using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Persistence;

namespace HotelManagement.WinForms.Services;

public class BookingService
{
    private readonly DataStore _store;
    private readonly RoomService _roomService;
    private readonly IPersistenceContext _persistence;

    public BookingService(
        DataStore store,
        RoomService roomService,
        IPersistenceContext? persistence = null)
    {
        _store = store;
        _roomService = roomService;
        _persistence = persistence ?? NullPersistenceContext.Instance;
    }

    public Reservation CreateReservation(
        Guest guest,
        Room room,
        DateTime checkIn,
        DateTime checkOut,
        List<AccompanyingGuest>? accompanying = null,
        string? marriageCertificateId = null)
    {
        // DC-2 / DEF-01: check-out must be strictly after check-in.
        if (checkOut <= checkIn)
            throw new ArgumentException(
                "Check-out date must be strictly after check-in date.",
                nameof(checkOut));

        var party = accompanying ?? new List<AccompanyingGuest>();

        // FR-RES-9 / DEF-02: party must fit room capacity (2A + C <= 2*Capacity).
        var adults   = 1 + party.Count(a => !a.IsChild);
        var children = party.Count(a => a.IsChild);
        var capacityUnits = 2 * adults + children;
        if (capacityUnits > 2 * room.Capacity)
            throw new InvalidOperationException(
                $"Room {room.Number} ({room.Type}) capacity exceeded: " +
                $"party uses {capacityUnits} unit(s), limit is {2 * room.Capacity}.");

        // FR-RES-10 / DEF-03: mixed-gender adult couple requires a marriage certificate ID.
        var hasOppositeGenderAdult =
            party.Any(a => !a.IsChild && a.Gender != guest.Gender);
        if (hasOppositeGenderAdult && string.IsNullOrWhiteSpace(marriageCertificateId))
            throw new InvalidOperationException(
                "A marriage certificate ID is required for a mixed-gender adult couple.");

        // DEF-08: no overlapping active reservations on the same room.
        var overlaps = _store.Reservations.Any(r =>
            r.Room == room &&
            r.Status != ReservationStatus.Cancelled &&
            r.Status != ReservationStatus.Completed &&
            checkIn < r.CheckOutDate &&
            r.CheckInDate < checkOut);
        if (overlaps)
            throw new InvalidOperationException(
                $"Room {room.Number} already has a reservation that overlaps these dates.");

        // New guests are persisted before the reservation that references them
        // so the FK insert order is correct on the SQL side.
        if (!_store.Guests.Contains(guest))
        {
            _store.Guests.Add(guest);
            _persistence.SaveGuest(guest);
        }
        else
        {
            _persistence.SaveGuest(guest);
        }

        var reservation = new Reservation
        {
            Guest = guest,
            Room = room,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            Status = ReservationStatus.Confirmed,
            Accompanying = party,
            MarriageCertificateId = marriageCertificateId
        };
        _store.Reservations.Add(reservation);
        _persistence.SaveReservation(reservation);
        return reservation;
    }

    public Stay CheckIn(Reservation reservation)
    {
        // FR-RES-4 / DEF-04: only a Confirmed reservation may be checked in.
        if (reservation.Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException(
                $"Reservation status is {reservation.Status}; only Confirmed reservations can be checked in.");

        // FR-ROOM-7 / DEF-05: the target room must not already be occupied.
        if (reservation.Room.IsOccupied)
            throw new InvalidOperationException(
                $"Room {reservation.Room.Number} is already occupied by another stay.");

        reservation.Status = ReservationStatus.CheckedIn;
        _persistence.SaveReservation(reservation);

        _roomService.MarkOccupied(reservation.Room); // writes Room through

        var stay = new Stay
        {
            Guest = reservation.Guest,
            Room = reservation.Room,
            CheckInDate = DateTime.Now,
            ExpectedCheckOut = reservation.CheckOutDate,
            Status = StayStatus.Active
        };
        _store.Stays.Add(stay);
        _persistence.SaveStay(stay);
        return stay;
    }

    public decimal CheckOut(Stay stay)
    {
        stay.ActualCheckOut = DateTime.Now;
        stay.Status = StayStatus.CheckedOut;

        // DEF-07: any portion of a day past check-in time counts as another night.
        // ceiling(0) = 0 so the Max(1, _) guarantees the minimum-one-night rule.
        var totalDays = (stay.ActualCheckOut.Value - stay.CheckInDate).TotalDays;
        var nights = Math.Max(1, (int)Math.Ceiling(totalDays));
        stay.RoomCharges = nights * stay.Room.Rate;

        // FR-RES-5 / DEF-06: cancelled orders must NOT be included in stay charges.
        stay.RestaurantCharges = _store.Orders
            .Where(o => o.Stay == stay && o.Status != OrderStatus.Cancelled)
            .Sum(o => o.Total);

        _roomService.MarkVacant(stay.Room);          // writes Room through
        _roomService.MarkNeedsCleaning(stay.Room);   // writes Room through
        stay.Guest.StayCount++;
        _persistence.SaveGuest(stay.Guest);
        _persistence.SaveStay(stay);

        var reservation = _store.Reservations.FirstOrDefault(r =>
            r.Guest == stay.Guest && r.Room == stay.Room && r.Status == ReservationStatus.CheckedIn);
        if (reservation != null)
        {
            reservation.Status = ReservationStatus.Completed;
            _persistence.SaveReservation(reservation);
        }

        return stay.TotalCharges;
    }

    public void Cancel(Reservation reservation)
    {
        reservation.Status = ReservationStatus.Cancelled;
        _persistence.SaveReservation(reservation);
    }
}
