using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Services;

public class RoomService
{
    private readonly DataStore _store;
    private readonly AuthService _auth;

    public RoomService(DataStore store, AuthService auth)
    {
        _store = store;
        _auth = auth;
    }

    public IEnumerable<Room> GetAvailableRooms() =>
        _store.Rooms.Where(r => r.IsAvailable);

    public void MarkOccupied(Room room) => room.IsOccupied = true;

    public void MarkVacant(Room room) => room.IsOccupied = false;

    public void MarkNeedsCleaning(Room room) => room.Condition = RoomCondition.NeedsCleaning;

    public void MarkClean(Room room) => room.Condition = RoomCondition.Clean;

    public void MarkOutOfService(Room room, string reason)
    {
        room.Condition = RoomCondition.OutOfService;
        room.MaintenanceLog = reason;
    }

    public Room AddRoom(int number, int floor, RoomType type, decimal rate)
    {
        _auth.Require(PermissionResource.Rooms, PermissionAction.Create);

        // DC-5 / DEF-09: rate must be non-negative.
        if (rate < 0)
            throw new ArgumentException("Room rate cannot be negative.", nameof(rate));

        // FR-ROOM-9 / DEF-10: floor must be non-negative.
        if (floor < 0)
            throw new ArgumentException("Floor cannot be negative.", nameof(floor));

        if (_store.Rooms.Any(r => r.Number == number))
            throw new InvalidOperationException($"Room {number} already exists.");

        var room = new Room { Number = number, Floor = floor, Type = type, Rate = rate };
        _store.Rooms.Add(room);
        return room;
    }

    public void UpdateRoom(Room room, int number, int floor, RoomType type, decimal rate)
    {
        _auth.Require(PermissionResource.Rooms, PermissionAction.Update);

        if (rate < 0)
            throw new ArgumentException("Room rate cannot be negative.", nameof(rate));
        if (floor < 0)
            throw new ArgumentException("Floor cannot be negative.", nameof(floor));

        if (_store.Rooms.Any(r => r.Number == number && r != room))
            throw new InvalidOperationException($"Room {number} already exists.");

        room.Number = number;
        room.Floor = floor;
        room.Type = type;
        room.Rate = rate;
    }

    public void RemoveRoom(Room room)
    {
        _auth.Require(PermissionResource.Rooms, PermissionAction.Delete);

        if (room.IsOccupied)
            throw new InvalidOperationException("Cannot remove an occupied room.");

        // FR-ROOM-4 (intent) / DEF-11: rooms with active reservations cannot be deleted.
        var hasActiveReservation = _store.Reservations.Any(r =>
            r.Room == room &&
            (r.Status == ReservationStatus.Pending ||
             r.Status == ReservationStatus.Confirmed ||
             r.Status == ReservationStatus.CheckedIn));
        if (hasActiveReservation)
            throw new InvalidOperationException(
                $"Cannot remove Room {room.Number}: one or more active reservations reference it.");

        _store.Rooms.Remove(room);
    }

    public IEnumerable<RoomType> GetRoomTypes() => Enum.GetValues<RoomType>();

    public Stay? GetCurrentStay(Room room)
    {
        return _store.Stays.FirstOrDefault(s => s.Room == room && s.Status == StayStatus.Active);
    }
}
