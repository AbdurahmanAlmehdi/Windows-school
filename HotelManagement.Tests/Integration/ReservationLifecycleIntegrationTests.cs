using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class ReservationLifecycleIntegrationTests
{
    [Fact]
    public void HappyPath_ReservationToCheckOutAndPaidInvoice()
    {
        var h = TestStoreFactory.Build();
        var guest = new Guest { Name = "Integration A", Passport = "INT-A" };
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        h.Store.Guests.Add(guest);

        // 1. Reservation
        var res = h.Booking.CreateReservation(guest, room,
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));
        res.Status.Should().Be(ReservationStatus.Confirmed);

        // 2. Check-in
        var stay = h.Booking.CheckIn(res);
        stay.Status.Should().Be(StayStatus.Active);

        // 3. Restaurant order during stay
        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[3], Quantity = 1 } });
        h.Restaurant.AdvanceOrderStatus(order);
        h.Restaurant.AdvanceOrderStatus(order);
        h.Restaurant.AdvanceOrderStatus(order);
        order.Status.Should().Be(OrderStatus.Served);

        // 4. Check-out — back-date so nights > 0
        stay.CheckInDate = DateTime.Now.AddDays(-3);
        h.Booking.CheckOut(stay);
        stay.Status.Should().Be(StayStatus.CheckedOut);
        room.Condition.Should().Be(RoomCondition.NeedsCleaning);
        res.Status.Should().Be(ReservationStatus.Completed);
        guest.StayCount.Should().Be(1);

        // 5. Generate + pay invoice
        var inv = h.Invoices.GenerateInvoice(stay);
        h.Invoices.MarkPaid(inv, PaymentMethod.CreditCard);
        inv.PaymentStatus.Should().Be(PaymentStatus.Paid);
        inv.Total.Should().Be(inv.Subtotal + inv.Tax);
    }

    [Fact]
    public void CannotDoubleBookSameRoom_OverlappingDates()
    {
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        var g1 = new Guest { Name = "Bob", Passport = "B1" };
        var g2 = new Guest { Name = "Carol", Passport = "C1" };

        h.Booking.CreateReservation(g1, room, DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));

        var act = () => h.Booking.CreateReservation(
            g2, room, DateTime.Today.AddDays(2), DateTime.Today.AddDays(3));


        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CancelledReservation_DoesNotAllowCheckIn()
    {
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        var guest = new Guest { Name = "Dave", Passport = "D1" };
        var res = h.Booking.CreateReservation(guest, room, DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        h.Booking.Cancel(res);

        var act = () => h.Booking.CheckIn(res);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CheckedOutStay_DoesNotAppearInActiveList()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);

        h.Booking.CheckOut(stay);

        h.Store.Stays.Where(s => s.Status == StayStatus.Active).Should().NotContain(stay);
    }
}
