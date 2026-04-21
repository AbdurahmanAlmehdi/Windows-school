using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Services;

public class BookingService
{
    private readonly DataStore _store;
    private readonly RoomService _roomService;

    public BookingService(DataStore store, RoomService roomService)
    {
        _store = store;
        _roomService = roomService;
    }

    public Reservation CreateReservation(Guest guest, Room room, DateTime checkIn, DateTime checkOut)
    {
        var reservation = new Reservation
        {
            Guest = guest,
            Room = room,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            Status = ReservationStatus.Confirmed
        };
        _store.Reservations.Add(reservation);
        return reservation;
    }

    public Stay CheckIn(Reservation reservation)
    {
        reservation.Status = ReservationStatus.CheckedIn;
        _roomService.MarkOccupied(reservation.Room);

        var stay = new Stay
        {
            Guest = reservation.Guest,
            Room = reservation.Room,
            CheckInDate = DateTime.Now,
            ExpectedCheckOut = reservation.CheckOutDate,
            Status = StayStatus.Active
        };
        _store.Stays.Add(stay);
        return stay;
    }

    public decimal CheckOut(Stay stay)
    {
        stay.ActualCheckOut = DateTime.Now;
        stay.Status = StayStatus.CheckedOut;

        var nights = Math.Max(1, (int)(stay.ActualCheckOut.Value - stay.CheckInDate).TotalDays);
        stay.RoomCharges = nights * stay.Room.Rate;

        var orders = _store.Orders.Where(o => o.Stay == stay);
        stay.RestaurantCharges = orders.Sum(o => o.Total);

        _roomService.MarkNeedsCleaning(stay.Room);
        stay.Guest.StayCount++;

        var reservation = _store.Reservations.FirstOrDefault(r =>
            r.Guest == stay.Guest && r.Room == stay.Room && r.Status == ReservationStatus.CheckedIn);
        if (reservation != null)
            reservation.Status = ReservationStatus.Completed;

        return stay.TotalCharges;
    }

    public void Cancel(Reservation reservation)
    {
        reservation.Status = ReservationStatus.Cancelled;
    }
}
