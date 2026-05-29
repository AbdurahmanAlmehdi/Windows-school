using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class RestaurantAndInvoiceIntegrationTests
{
    [Fact]
    public void OrderLines_FlowIntoInvoice()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var salad = h.Store.MenuItems.First(m => m.Name == "Caesar Salad");
        var coffee = h.Store.MenuItems.First(m => m.Name == "Cappuccino");

        var order = h.Restaurant.CreateOrder(stay, new[]
        {
            new OrderLine { MenuItem = salad,  Quantity = 1 },
            new OrderLine { MenuItem = coffee, Quantity = 2 }
        });
        for (int i = 0; i < 3; i++) h.Restaurant.AdvanceOrderStatus(order);

        stay.CheckInDate = DateTime.Now.AddDays(-2);
        h.Booking.CheckOut(stay);

        var inv = h.Invoices.GenerateInvoice(stay);

        var restaurantLines = inv.Lines.Where(l => l.Category == InvoiceLineCategory.RestaurantCharge).ToList();
        restaurantLines.Should().Contain(l => l.Description == "Caesar Salad");
        restaurantLines.Should().Contain(l => l.Description == "Cappuccino" && l.Quantity == 2);
    }

    [Fact]
    public void RefundFlow_RevertsPaidInvoice_PerFR_INV_6()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.CheckInDate = DateTime.Now.AddDays(-1);
        h.Booking.CheckOut(stay);
        var inv = h.Invoices.GenerateInvoice(stay);
        h.Invoices.MarkPaid(inv, PaymentMethod.DebitCard);

        h.Invoices.MarkRefunded(inv);

        inv.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void DashboardRevenue_AggregatesAcrossPaidInvoices()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.CheckInDate = DateTime.Now.AddDays(-2);
        h.Booking.CheckOut(stay);
        var inv = h.Invoices.GenerateInvoice(stay);
        var before = h.Invoices.GetTotalRevenue();

        h.Invoices.MarkPaid(inv, PaymentMethod.Cash);

        h.Invoices.GetTotalRevenue().Should().Be(before + inv.Total);
    }

    [Fact]
    public void CancelledOrder_DoesNotIncreaseStayCharges()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        var beforeCharges = stay.RestaurantCharges;
        var item = h.Store.MenuItems[0];

        var order = h.Restaurant.CreateOrder(stay,
            new[] { new OrderLine { MenuItem = item, Quantity = 5 } });
        h.Restaurant.AddLinesToOrder(order, new[]
        {
            new OrderLine { MenuItem = h.Store.MenuItems[1], Quantity = 2 }
        });
        h.Restaurant.CancelOrder(order);

        h.Restaurant.AddLinesToOrder(order, Array.Empty<OrderLine>());

        stay.RestaurantCharges.Should().Be(beforeCharges);
    }
}
