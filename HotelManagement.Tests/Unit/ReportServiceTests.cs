using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class ReportServiceTests
{
    [Fact]
    public void GetOccupancyRate_ReturnsZero_WhenNoRooms()
    {
        var store = new DataStore();
        while (store.Rooms.Count > 0) store.Rooms.RemoveAt(0);
        var rs = new ReportService(store);

        rs.GetOccupancyRate().Should().Be(0);
    }

    [Fact]
    public void GetOccupancyRate_ReflectsOccupiedShare_PerFR_RPT_1()
    {
        var h = TestStoreFactory.Build();
        var totalRooms = h.Store.Rooms.Count;
        var occupied = h.Store.Rooms.Count(r => r.IsOccupied);

        h.Reports.GetOccupancyRate().Should().BeApproximately((double)occupied / totalRooms * 100, 0.001);
    }

    [Fact]
    public void GetRevenueByRoomType_GroupsCorrectly_PerFR_RPT_2()
    {
        var h = TestStoreFactory.Build();

        var dict = h.Reports.GetRevenueByRoomType();

        dict.Should().NotBeEmpty();
        foreach (var kvp in dict)
            kvp.Value.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void GetTopMenuItems_OrdersByQuantityDescending_PerFR_RPT_4()
    {
        var h = TestStoreFactory.Build();

        var top = h.Reports.GetTopMenuItems(3);

        top.Should().BeInDescendingOrder(t => t.Count);
        top.Should().HaveCountLessOrEqualTo(3);
    }

    [Fact]
    public void GetRepeatGuestPercentage_ReflectsSeed_PerFR_RPT_6()
    {
        var h = TestStoreFactory.Build();
        var expected = (double)h.Store.Guests.Count(g => g.StayCount > 1)
                       / h.Store.Guests.Count * 100;

        h.Reports.GetRepeatGuestPercentage().Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public void GetAverageStayDuration_IgnoresActiveStays_PerFR_RPT_5()
    {
        var h = TestStoreFactory.Build();
        var checkedOut = h.Store.Stays.Where(s => s.Status == StayStatus.CheckedOut).ToList();
        var expected = checkedOut.Average(s => (s.ActualCheckOut!.Value - s.CheckInDate).TotalDays);

        h.Reports.GetAverageStayDuration().Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public void GetAverageStayDuration_ReturnsZero_WhenNoCheckedOutStays()
    {
        var h = TestStoreFactory.Build();
        while (h.Store.Stays.Count > 0) h.Store.Stays.RemoveAt(0);

        h.Reports.GetAverageStayDuration().Should().Be(0);
    }

    [Fact]
    public void GetRestaurantRevenueByCategory_AggregatesAcrossOrders_PerFR_RPT_3()
    {
        var h = TestStoreFactory.Build();

        var dict = h.Reports.GetRestaurantRevenueByCategory();

        dict.Should().NotBeEmpty();
        dict.Values.Sum().Should().BeGreaterThan(0);
    }
}
