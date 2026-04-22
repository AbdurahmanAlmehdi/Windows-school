using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Services;

public class RoomService
{
    private readonly DataStore _store;

    public RoomService(DataStore store)
    {
        _store = store;
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

    public Room AddRoom(int number, RoomType type, decimal rate)
    {
        if (_store.Rooms.Any(r => r.Number == number))
            throw new InvalidOperationException($"Room {number} already exists.");

        var room = new Room { Number = number, Type = type, Rate = rate };
        _store.Rooms.Add(room);
        return room;
    }

    public void UpdateRoom(Room room, int number, RoomType type, decimal rate)
    {
        if (_store.Rooms.Any(r => r.Number == number && r != room))
            throw new InvalidOperationException($"Room {number} already exists.");

        room.Number = number;
        room.Type = type;
        room.Rate = rate;
    }

    public void RemoveRoom(Room room)
    {
        if (room.IsOccupied)
            throw new InvalidOperationException("Cannot remove an occupied room.");

        _store.Rooms.Remove(room);
    }

    public IEnumerable<RoomType> GetRoomTypes() => Enum.GetValues<RoomType>();

    public Stay? GetCurrentStay(Room room)
    {
        return _store.Stays.FirstOrDefault(s => s.Room == room && s.Status == StayStatus.Active);
    }
}
