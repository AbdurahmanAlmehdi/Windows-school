using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Services;

public class ReportService
{
    private readonly DataStore _store;

    public ReportService(DataStore store)
    {
        _store = store;
    }

    public double GetOccupancyRate()
    {
        if (_store.Rooms.Count == 0) return 0;
        return (double)_store.Rooms.Count(r => r.IsOccupied) / _store.Rooms.Count * 100;
    }

    public Dictionary<RoomType, decimal> GetRevenueByRoomType()
    {
        return _store.Stays
            .GroupBy(s => s.Room.Type)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.RoomCharges));
    }

    public Dictionary<string, decimal> GetRestaurantRevenueByCategory()
    {
        return _store.Orders
            .SelectMany(o => o.Lines)
            .GroupBy(l => l.MenuItem.Category)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.LineTotal));
    }

    public List<(string Name, int Count)> GetTopMenuItems(int top = 5)
    {
        return _store.Orders
            .SelectMany(o => o.Lines)
            .GroupBy(l => l.MenuItem.Name)
            .OrderByDescending(g => g.Sum(l => l.Quantity))
            .Take(top)
            .Select(g => (g.Key, g.Sum(l => l.Quantity)))
            .ToList();
    }

    public double GetAverageStayDuration()
    {
        var completed = _store.Stays.Where(s => s.Status == StayStatus.CheckedOut && s.ActualCheckOut.HasValue).ToList();
        if (!completed.Any()) return 0;
        return completed.Average(s => (s.ActualCheckOut!.Value - s.CheckInDate).TotalDays);
    }

    public double GetRepeatGuestPercentage()
    {
        if (_store.Guests.Count == 0) return 0;
        return (double)_store.Guests.Count(g => g.StayCount > 1) / _store.Guests.Count * 100;
    }
}
