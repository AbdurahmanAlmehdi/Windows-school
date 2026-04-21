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
        _store.Rooms.Where(r => r.Status == RoomStatus.Available);

    public void MarkOccupied(Room room) => room.Status = RoomStatus.Occupied;

    public void MarkNeedsCleaning(Room room) => room.Status = RoomStatus.NeedsCleaning;

    public void MarkClean(Room room) => room.Status = RoomStatus.Available;

    public void MarkOutOfService(Room room, string reason)
    {
        room.Status = RoomStatus.OutOfService;
        room.MaintenanceLog = reason;
    }
}
