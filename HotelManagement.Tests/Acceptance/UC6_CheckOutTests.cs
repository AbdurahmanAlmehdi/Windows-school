using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Acceptance;

// =====================================================================
// UC-6: Complete Guest Check-out (End-to-End Journey)
// =====================================================================
//
// As a front-desk staff member,
// I want to walk a guest through the full reserve → check-in → order →
// check-out → pay journey,
// so that their stay is closed and the room is returned to inventory.
//
// Acceptance criteria covered:
//   - The complete happy-path UC-2 → UC-3 → UC-4 → UC-5 → UC-6 flow
//     leaves the system in a consistent state.
//   - On check-out: stay becomes CheckedOut, room becomes vacant +
//     NeedsCleaning, guest's stay count increments by 1, reservation
//     transitions to Completed (FR-RES-5, FR-RES-6, FR-ROOM-8).
//   - Restaurant charges aggregate non-cancelled orders only
//     (FR-RES-5 / DEF-06).
//   - Partial nights past check-in time count as another full night
//     (DEF-07 / Math.Ceiling).
// =====================================================================

public class UC6_CheckOutTests
{
    [Fact]
    public void Scenario_FullGuestJourney_ReserveCheckInOrderCheckOutPay_LeavesConsistentState()
    {
        // Given a fresh hotel session with a walk-in guest
        var h = TestStoreFactory.Build();
        var guest = new Guest { Name = "Integration Journey", Passport = "P-JRN" };
        var room  = TestStoreFactory.FirstAvailableRoom(h.Store);
        h.Store.Guests.Add(guest);

        // Step 1: reserve the room
        var res = h.Booking.CreateReservation(guest, room,
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));
        res.Status.Should().Be(ReservationStatus.Confirmed);

        // Step 2: check the guest in
        var stay = h.Booking.CheckIn(res);
        stay.Status.Should().Be(StayStatus.Active);
        room.IsOccupied.Should().BeTrue();

        // Step 3: place a restaurant order and serve it
        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[3], Quantity = 1 } });
        h.Restaurant.AdvanceOrderStatus(order);   // Preparing
        h.Restaurant.AdvanceOrderStatus(order);   // Ready
        h.Restaurant.AdvanceOrderStatus(order);   // Served
        order.Status.Should().Be(OrderStatus.Served);

        // Step 4: check the guest out (we backdate so the night count is real)
        stay.CheckInDate = DateTime.Now.AddDays(-3);
        h.Booking.CheckOut(stay);
        stay.Status.Should().Be(StayStatus.CheckedOut);
        room.IsOccupied.Should().BeFalse();
        room.Condition.Should().Be(RoomCondition.NeedsCleaning);
        res.Status.Should().Be(ReservationStatus.Completed);
        guest.StayCount.Should().Be(1);

        // Step 5: generate and pay the folio
        var inv = h.Invoices.GenerateInvoice(stay);
        h.Invoices.MarkPaid(inv, PaymentMethod.CreditCard);
        inv.PaymentStatus.Should().Be(PaymentStatus.Paid);
        inv.Total.Should().Be(inv.Subtotal + inv.Tax);
    }

    [Fact]
    public void Scenario_CheckOutAtSixPMOnSecondDay_GuestIsBilledForTwoNights_PerHotelConvention()
    {
        // Given a stay that began 30 hours ago (1.25 days)
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        var stay = new Stay
        {
            Guest = new Guest { Name = "Late Departure", Passport = "P-LATE" },
            Room  = room,
            CheckInDate     = DateTime.Now.AddHours(-30),
            ExpectedCheckOut = DateTime.Now,
            Status = StayStatus.Active
        };
        h.Store.Stays.Add(stay);
        h.Rooms.MarkOccupied(room);

        // When the staff checks the guest out
        h.Booking.CheckOut(stay);

        // Then they are billed for two full nights (1.25 -> ceil -> 2)
        stay.RoomCharges.Should().Be(2 * room.Rate);
    }

    [Fact]
    public void Scenario_CheckOutImmediatelyAfterCheckIn_GuestIsBilledForAtLeastOneNight()
    {
        // Given a stay that just started
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.CheckInDate = DateTime.Now;

        // When the staff checks them out a moment later
        h.Booking.CheckOut(stay);

        // Then the guest is still billed for one full night
        stay.RoomCharges.Should().BeGreaterOrEqualTo(stay.Room.Rate);
    }

    [Fact]
    public void Scenario_CheckOutAggregatesNonCancelledRestaurantCharges_PerFR_RES_5()
    {
        // Given an active stay with one live and one cancelled restaurant order
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var existingNonCancelled = h.Store.Orders
            .Where(o => o.Stay == stay && o.Status != OrderStatus.Cancelled)
            .Sum(o => o.Total);
        var menuItem = h.Store.MenuItems[0];
        var live = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = menuItem, Quantity = 1 } });
        var cancelled = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = menuItem, Quantity = 5 } });
        h.Restaurant.CancelOrder(cancelled);

        // When the guest checks out
        h.Booking.CheckOut(stay);

        // Then the cancelled order is excluded from restaurant charges
        stay.RestaurantCharges.Should().Be(existingNonCancelled + live.Total);
    }

    [Fact]
    public void Scenario_AfterCheckOut_StayIsNoLongerInActiveList_PerFR_RES_6()
    {
        // Given an active stay
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);

        // When the guest checks out
        h.Booking.CheckOut(stay);

        // Then the stay is no longer shown as Active
        h.Store.Stays
            .Where(s => s.Status == StayStatus.Active)
            .Should().NotContain(stay);
    }
}
