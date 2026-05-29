using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class RestaurantServiceTests
{
    [Fact]
    public void CreateOrder_StoresOrderInPlacedState_PerFR_RST_6()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var item = h.Store.MenuItems[0];

        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = item, Quantity = 2 } });

        order.Status.Should().Be(OrderStatus.Placed);
        order.Lines.Should().HaveCount(1);
        h.Store.Orders.Should().Contain(order);
    }

    [Fact]
    public void CreateOrder_RejectsCheckedOutStay_PerFR_RST_4()
    {
        var h = TestStoreFactory.Build();
        var pastStay = h.Store.Stays.First(s => s.Status == StayStatus.CheckedOut);
        var item = h.Store.MenuItems[0];

        var act = () => h.Restaurant.CreateOrder(pastStay, new[] { new OrderLine { MenuItem = item, Quantity = 1 } });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AdvanceOrderStatus_FollowsLegalStateMachine_PerFR_RST_6()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });

        h.Restaurant.AdvanceOrderStatus(order);
        order.Status.Should().Be(OrderStatus.Preparing);
        h.Restaurant.AdvanceOrderStatus(order);
        order.Status.Should().Be(OrderStatus.Ready);
        h.Restaurant.AdvanceOrderStatus(order);
        order.Status.Should().Be(OrderStatus.Served);
    }

    [Fact]
    public void AdvanceOrderStatus_StopsAtServed_PerFR_RST_6()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });
        for (int i = 0; i < 5; i++) h.Restaurant.AdvanceOrderStatus(order);

        order.Status.Should().Be(OrderStatus.Served);
    }

    [Fact]
    public void AdvanceOrderStatus_RejectsCancelledOrder()
    {

        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });
        h.Restaurant.CancelOrder(order);

        var act = () => h.Restaurant.AdvanceOrderStatus(order);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CancelOrder_AllowedFromPlaced_PerFR_RST_7()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });

        h.Restaurant.CancelOrder(order);

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void CancelOrder_AllowedFromPreparing_PerFR_RST_7()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });
        h.Restaurant.AdvanceOrderStatus(order); // Preparing

        h.Restaurant.CancelOrder(order);

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void CancelOrder_RejectedFromReadyOrServed_PerFR_RST_7()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });
        h.Restaurant.AdvanceOrderStatus(order);
        h.Restaurant.AdvanceOrderStatus(order);

        h.Restaurant.CancelOrder(order);

        order.Status.Should().Be(OrderStatus.Ready);
    }

    [Fact]
    public void AddLinesToOrder_RecomputesStayRestaurantCharges_PerFR_RST_8()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });
        var beforeAdd = stay.RestaurantCharges;
        var add = new[] { new OrderLine { MenuItem = h.Store.MenuItems[1], Quantity = 2 } };

        h.Restaurant.AddLinesToOrder(order, add);

        stay.RestaurantCharges.Should().BeGreaterThan(beforeAdd);
        order.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void AddLinesToOrder_NoOp_WhenOrderServed()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var order = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 1 } });
        for (int i = 0; i < 3; i++) h.Restaurant.AdvanceOrderStatus(order);
        var beforeLines = order.Lines.Count;

        h.Restaurant.AddLinesToOrder(order, new[] { new OrderLine { MenuItem = h.Store.MenuItems[1], Quantity = 1 } });

        order.Lines.Count.Should().Be(beforeLines);
    }

    [Fact]
    public void AddMenuItem_RequiresCreatePermission_PerNFR_SEC_3()
    {
        var h = TestStoreFactory.Build(loginAs: "staff", password: "staff123");

        var act = () => h.Restaurant.AddMenuItem(new MenuItem { Name = "Hack", Price = 1m });

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void AddMenuItem_AcceptedAsManager()
    {
        var h = TestStoreFactory.Build(); // superadmin

        h.Restaurant.AddMenuItem(new MenuItem { Name = "Mojito", Price = 8.50m, Category = "Beverages" });

        h.Store.MenuItems.Should().Contain(m => m.Name == "Mojito");
    }

    [Fact]
    public void ToggleAvailability_Flips()
    {
        var h = TestStoreFactory.Build();
        var item = h.Store.MenuItems[0];
        var before = item.IsAvailable;

        h.Restaurant.ToggleAvailability(item);

        item.IsAvailable.Should().Be(!before);
    }

    [Fact]
    public void OrderLine_RejectsZeroQuantity_PerDC_4()
    {
        // DC-4: OrderLine.Quantity >= 1. Negative or zero must be rejected.
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);

        var act = () => h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = 0 } });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OrderLine_RejectsNegativeQuantity_PerDC_4()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);

        var act = () => h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = h.Store.MenuItems[0], Quantity = -3 } });

        act.Should().Throw<ArgumentException>();
    }
}
