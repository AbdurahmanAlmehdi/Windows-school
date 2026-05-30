using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Acceptance;

// =====================================================================
// UC-4: Take a Restaurant Order During a Stay
// =====================================================================
//
// As a restaurant staff member,
// I want to record items ordered by an in-house guest,
// so that the charges flow into the guest's folio at check-out.
//
// Acceptance criteria covered:
//   - An order can be opened against an Active stay (FR-RST-4).
//   - The order advances Placed → Preparing → Ready → Served (FR-RST-6).
//   - A Served order is the terminal state; further advances no-op.
//   - An order can only be cancelled from Placed or Preparing (FR-RST-7).
//   - A cancelled order does not contribute to stay charges (FR-RST-7,
//     FR-RES-5 / DEF-06).
//   - Order lines have a quantity of at least 1 (DC-4 / DEF-14).
//   - Orders cannot be opened against a CheckedOut stay (DEF-12).
// =====================================================================

public class UC4_RestaurantTests
{
    [Fact]
    public void Scenario_WaiterPlacesOrderForInHouseGuest_OrderIsRecordedAsPlaced()
    {
        // Given an active stay
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var menuItem = h.Store.MenuItems[0];

        // When the waiter places a one-item order
        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = menuItem, Quantity = 1 } });

        // Then the order is recorded against the stay in Placed status
        order.Status.Should().Be(OrderStatus.Placed);
        order.Stay.Should().Be(stay);
        h.Store.Orders.Should().Contain(order);
    }

    [Fact]
    public void Scenario_KitchenAdvancesOrderThroughLifecycle_StatusReachesServed_PerFR_RST_6()
    {
        // Given a placed order
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });

        // When the kitchen advances the order three times
        h.Restaurant.AdvanceOrderStatus(order);   // Preparing
        h.Restaurant.AdvanceOrderStatus(order);   // Ready
        h.Restaurant.AdvanceOrderStatus(order);   // Served

        // Then the order reaches Served
        order.Status.Should().Be(OrderStatus.Served);
    }

    [Fact]
    public void Scenario_GuestRequestsItemAlreadyServed_OrderStaysAtServed()
    {
        // Given an order that already reached Served
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });
        for (int i = 0; i < 3; i++) h.Restaurant.AdvanceOrderStatus(order);

        // When the kitchen presses Advance again by mistake
        h.Restaurant.AdvanceOrderStatus(order);

        // Then the order stays at Served (idempotent terminal state)
        order.Status.Should().Be(OrderStatus.Served);
    }

    [Fact]
    public void Scenario_GuestCancelsOrderBeforeItIsReady_OrderTransitionsToCancelled()
    {
        // Given an order that is being prepared
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 2 } });
        h.Restaurant.AdvanceOrderStatus(order);   // Preparing

        // When the guest cancels
        h.Restaurant.CancelOrder(order);

        // Then the order becomes Cancelled
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Scenario_GuestTriesToCancelAlreadyReadyOrder_CancellationIsRejected_PerFR_RST_7()
    {
        // Given an order that has been plated and is Ready for delivery
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });
        h.Restaurant.AdvanceOrderStatus(order);   // Preparing
        h.Restaurant.AdvanceOrderStatus(order);   // Ready

        // When the guest tries to cancel
        h.Restaurant.CancelOrder(order);

        // Then the order stays Ready (silent no-op per FR-RST-7)
        order.Status.Should().Be(OrderStatus.Ready);
    }

    [Fact]
    public void Scenario_OrderCancelled_StayChargesDoNotIncludeIt_PerFR_RES_5()
    {
        // Given an in-house guest with no prior orders
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStayWithoutOrders(h.Store);
        var beforeCharges = stay.RestaurantCharges;
        var menuItem = h.Store.MenuItems[0];

        // When the waiter creates an order, then the guest cancels it
        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = menuItem, Quantity = 5 } });
        h.Restaurant.AddLinesToOrder(order,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[1], Quantity = 2 } });
        h.Restaurant.CancelOrder(order);

        // Then the stay's restaurant charges return to their pre-order value
        stay.RestaurantCharges.Should().Be(beforeCharges);
    }

    [Fact]
    public void Scenario_OrderLineWithZeroQuantity_OrderIsRejected_PerDC_4()
    {
        // Given an active stay
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);

        // When a waiter accidentally enters quantity 0
        var act = () => h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 0 } });

        // Then the system rejects the order
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Scenario_AttemptOrderAgainstCheckedOutStay_OrderIsRejected_PerFR_RST_4()
    {
        // Given a stay that is already checked out
        var h = TestStoreFactory.Build();
        var pastStay = h.Store.Stays.First(s => s.Status == StayStatus.CheckedOut);

        // When a waiter tries to attach a new order to it
        var act = () => h.Restaurant.CreateOrder(pastStay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });

        // Then the system rejects the order
        act.Should().Throw<InvalidOperationException>();
    }
}
