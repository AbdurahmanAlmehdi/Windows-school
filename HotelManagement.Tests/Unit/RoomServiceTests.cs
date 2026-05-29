using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class RoomServiceTests
{
    [Fact]
    public void GetAvailableRooms_ExcludesOccupiedRooms()
    {
        var h = TestStoreFactory.Build();

        var available = h.Rooms.GetAvailableRooms().ToList();

        available.Should().NotBeEmpty();
        available.Should().OnlyContain(r => !r.IsOccupied);
    }

    [Fact]
    public void GetAvailableRooms_ExcludesNeedsCleaning()
    {
        var h = TestStoreFactory.Build();

        var available = h.Rooms.GetAvailableRooms().ToList();

        available.Should().OnlyContain(r => r.Condition == RoomCondition.Clean);
    }

    [Fact]
    public void GetAvailableRooms_ExcludesOutOfService()
    {
        var h = TestStoreFactory.Build();
        h.Store.Rooms.Should().Contain(r => r.Condition == RoomCondition.OutOfService);

        var available = h.Rooms.GetAvailableRooms().ToList();

        available.Should().NotContain(r => r.Condition == RoomCondition.OutOfService);
    }

    [Fact]
    public void AddRoom_RejectsDuplicateNumber_PerFR_ROOM_2()
    {
        var h = TestStoreFactory.Build();
        var existing = h.Store.Rooms[0].Number;

        var act = () => h.Rooms.AddRoom(existing, 9, RoomType.Single, 100m);

        act.Should().Throw<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public void AddRoom_AcceptsNewUniqueNumber()
    {
        var h = TestStoreFactory.Build();

        var room = h.Rooms.AddRoom(999, 9, RoomType.Suite, 175m);

        room.Number.Should().Be(999);
        h.Store.Rooms.Should().Contain(room);
    }

    [Fact]
    public void AddRoom_RejectsNegativeRate()
    {
        var h = TestStoreFactory.Build();

        var act = () => h.Rooms.AddRoom(900, 9, RoomType.Single, -50m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRoom_RejectsNegativeFloor()
    {
        var h = TestStoreFactory.Build();

        var act = () => h.Rooms.AddRoom(901, -3, RoomType.Single, 100m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRoom_RequiresCreatePermission()
    {
        var h = TestStoreFactory.Build(loginAs: "staff", password: "staff123");

        var act = () => h.Rooms.AddRoom(950, 9, RoomType.Single, 100m);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void RemoveRoom_RejectsOccupiedRoom_PerFR_ROOM_4()
    {
        var h = TestStoreFactory.Build();
        var occupied = h.Store.Rooms.First(r => r.IsOccupied);

        var act = () => h.Rooms.RemoveRoom(occupied);

        act.Should().Throw<InvalidOperationException>().WithMessage("*occupied*");
    }

    [Fact]
    public void RemoveRoom_RejectsRoomWithConfirmedReservation()
    {
        var h = TestStoreFactory.Build();
        var booked = h.Store.Reservations
            .First(r => r.Status == ReservationStatus.Confirmed).Room;

        var act = () => h.Rooms.RemoveRoom(booked);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveRoom_RemovesVacantCleanRoom()
    {
        var h = TestStoreFactory.Build();
        var vacant = h.Store.Rooms.First(r => r.IsAvailable);

        h.Rooms.RemoveRoom(vacant);

        h.Store.Rooms.Should().NotContain(vacant);
    }

    [Fact]
    public void MarkOccupied_SetsFlag()
    {
        var h = TestStoreFactory.Build();
        var room = h.Store.Rooms.First(r => r.IsAvailable);

        h.Rooms.MarkOccupied(room);

        room.IsOccupied.Should().BeTrue();
    }

    [Fact]
    public void MarkOutOfService_StoresMaintenanceReason()
    {
        var h = TestStoreFactory.Build();
        var room = h.Store.Rooms.First(r => r.IsAvailable);

        h.Rooms.MarkOutOfService(room, "broken HVAC");

        room.Condition.Should().Be(RoomCondition.OutOfService);
        room.MaintenanceLog.Should().Be("broken HVAC");
    }

    [Fact]
    public void UpdateRoom_RejectsDuplicateNumber()
    {
        var h = TestStoreFactory.Build();
        var first = h.Store.Rooms[0];
        var second = h.Store.Rooms[1];

        var act = () => h.Rooms.UpdateRoom(second, first.Number, second.Floor, second.Type, second.Rate);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RoomCapacity_MatchesSRS_FR_ROOM_10()
    {
        var h = TestStoreFactory.Build();

        h.Store.Rooms.First(r => r.Type == RoomType.Single).Capacity.Should().Be(1);
        h.Store.Rooms.First(r => r.Type == RoomType.Double).Capacity.Should().Be(2);
        h.Store.Rooms.First(r => r.Type == RoomType.Suite).Capacity.Should().Be(4);
        h.Store.Rooms.First(r => r.Type == RoomType.Deluxe).Capacity.Should().Be(4);
        h.Store.Rooms.First(r => r.Type == RoomType.Penthouse).Capacity.Should().Be(6);
    }
}
