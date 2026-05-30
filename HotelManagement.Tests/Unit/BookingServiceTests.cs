using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class BookingServiceTests
{
    [Fact]
    public void CreateReservation_StoresAsConfirmed_PerFR_RES_2()
    {
        var h = TestStoreFactory.Build();
        var guest = h.Store.Guests[0];
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);

        var r = h.Booking.CreateReservation(
            guest, room, DateTime.Today.AddDays(10), DateTime.Today.AddDays(12));

        r.Status.Should().Be(ReservationStatus.Confirmed);
        h.Store.Reservations.Should().Contain(r);
    }

    [Fact]
    public void CreateReservation_RejectsCheckOutBeforeCheckIn_PerDC_2()
    {
        var h = TestStoreFactory.Build();
        var guest = h.Store.Guests[0];
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);

        var act = () => h.Booking.CreateReservation(
            guest, room, DateTime.Today.AddDays(5), DateTime.Today.AddDays(2));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateReservation_RejectsZeroNightStay_PerDC_2()
    {
        var h = TestStoreFactory.Build();
        var guest = h.Store.Guests[0];
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        var day = DateTime.Today.AddDays(5);

        var act = () => h.Booking.CreateReservation(guest, room, day, day);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateReservation_RejectsOverCapacityParty_PerFR_RES_9()
    {
        var h = TestStoreFactory.Build();
        var single = h.Store.Rooms.First(r => r.Type == RoomType.Single && r.IsAvailable);
        var guest = h.Store.Guests[0];
        var party = new List<AccompanyingGuest>
        {
            new() { Name = "Spouse", Age = 30, Gender = Gender.Female, Passport = "P1" }
        };

        var act = () => h.Booking.CreateReservation(
            guest, single, DateTime.Today.AddDays(3), DateTime.Today.AddDays(5),
            accompanying: party);

        act.Should().Throw<InvalidOperationException>().WithMessage("*capacity*");
    }

    [Fact]
    public void CreateReservation_RequiresMarriageCertificate_WhenMixedGenderAdults_PerFR_RES_10()
    {
        var h = TestStoreFactory.Build();
        var room = h.Store.Rooms.First(r => r.IsAvailable && r.Capacity >= 2);
        var primary = new Guest { Name = "Alex", Gender = Gender.Male, Passport = "X1" };
        var party = new List<AccompanyingGuest>
        {
            new() { Name = "Sam", Gender = Gender.Female, Age = 29, Passport = "X2" }
        };

        var act = () => h.Booking.CreateReservation(
            primary, room, DateTime.Today.AddDays(2), DateTime.Today.AddDays(4),
            accompanying: party,
            marriageCertificateId: null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*marriage*");
    }

    [Fact]
    public void CreateReservation_AllowsSameGenderAdults_WithoutCertificate()
    {
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store, minCapacity: 2);
        var primary = new Guest { Name = "Pat", Gender = Gender.Male, Passport = "X1" };
        var party = new List<AccompanyingGuest>
        {
            new() { Name = "Chris", Gender = Gender.Male, Age = 35, Passport = "X2" }
        };

        var act = () => h.Booking.CreateReservation(
            primary, room, DateTime.Today.AddDays(2), DateTime.Today.AddDays(4),
            accompanying: party);

        act.Should().NotThrow();
    }

    [Fact]
    public void CreateReservation_AllowsOppositeGenderChild_WithoutCertificate_PerFR_RES_10()
    {
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store, minCapacity: 2);
        var primary = new Guest { Name = "Adult", Gender = Gender.Male, Passport = "X1" };
        var party = new List<AccompanyingGuest>
        {
            new() { Name = "Daughter", Gender = Gender.Female, Age = 9, Passport = "X2" }
        };

        var act = () => h.Booking.CreateReservation(
            primary, room, DateTime.Today.AddDays(2), DateTime.Today.AddDays(4),
            accompanying: party);

        act.Should().NotThrow();
    }

    [Fact]
    public void CheckIn_SetsReservationToCheckedIn_AndCreatesActiveStay_PerFR_RES_4()
    {
        var h = TestStoreFactory.Build();
        var res = h.Store.Reservations.First(r => r.Status == ReservationStatus.Confirmed);

        var stay = h.Booking.CheckIn(res);

        res.Status.Should().Be(ReservationStatus.CheckedIn);
        stay.Status.Should().Be(StayStatus.Active);
        stay.Room.Should().Be(res.Room);
        stay.Room.IsOccupied.Should().BeTrue();
        h.Store.Stays.Should().Contain(stay);
    }

    [Fact]
    public void CheckIn_RejectsCancelledReservation()
    {
        var h = TestStoreFactory.Build();
        var res = h.Store.Reservations.First(r => r.Status == ReservationStatus.Confirmed);
        h.Booking.Cancel(res);

        var act = () => h.Booking.CheckIn(res);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CheckIn_RejectsAlreadyOccupiedRoom_PerFR_ROOM_7()
    {

        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        var g1 = new Guest { Name = "G1", Passport = "P1" };
        var g2 = new Guest { Name = "G2", Passport = "P2" };

        var r1 = h.Booking.CreateReservation(g1, room, DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        var r2 = h.Booking.CreateReservation(g2, room, DateTime.Today.AddDays(4), DateTime.Today.AddDays(6));

        h.Booking.CheckIn(r1);

        var act = () => h.Booking.CheckIn(r2);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CheckOut_TransitionsStay_AndRoomCondition_PerFR_RES_5_AndFR_ROOM_8()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var initialStayCount = stay.Guest.StayCount;

        h.Booking.CheckOut(stay);

        stay.Status.Should().Be(StayStatus.CheckedOut);
        stay.ActualCheckOut.Should().NotBeNull();
        stay.Room.IsOccupied.Should().BeFalse();
        stay.Room.Condition.Should().Be(RoomCondition.NeedsCleaning);
        stay.Guest.StayCount.Should().Be(initialStayCount + 1);
    }

    [Fact]
    public void CheckOut_ChargesAtLeastOneNight_PerFR_RES_5()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.CheckInDate = DateTime.Now;

        h.Booking.CheckOut(stay);

        stay.RoomCharges.Should().BeGreaterOrEqualTo(stay.Room.Rate);
    }

    [Fact]
    public void CheckOut_RoundsUpPartialNight_PerHotelConvention()
    {
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        var stay = new Stay
        {
            Guest = new Guest { Name = "Late", Passport = "P9" },
            Room = room,
            CheckInDate = DateTime.Now.AddHours(-30),
            ExpectedCheckOut = DateTime.Now,
            Status = StayStatus.Active
        };
        h.Store.Stays.Add(stay);
        h.Rooms.MarkOccupied(room);

        h.Booking.CheckOut(stay);

        stay.RoomCharges.Should().Be(2 * room.Rate);
    }

    [Fact]
    public void CheckOut_AggregatesNonCancelledRestaurantCharges_PerFR_RES_5()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var existing = h.Store.Orders
            .Where(o => o.Stay == stay && o.Status != OrderStatus.Cancelled)
            .Sum(o => o.Total);
        var item = h.Store.MenuItems[0];

        var live = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = item, Quantity = 1 } });
        var cancelled = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = item, Quantity = 5 } });
        h.Restaurant.CancelOrder(cancelled);

        h.Booking.CheckOut(stay);

        stay.RestaurantCharges.Should().Be(existing + live.Total,
            "cancelled orders must be excluded per FR-RES-5");
    }

    [Fact]
    public void Cancel_TransitionsConfirmedToCancelled_PerFR_RES_3()
    {
        var h = TestStoreFactory.Build();
        var res = h.Store.Reservations.First(r => r.Status == ReservationStatus.Confirmed);

        h.Booking.Cancel(res);

        res.Status.Should().Be(ReservationStatus.Cancelled);
    }
}
