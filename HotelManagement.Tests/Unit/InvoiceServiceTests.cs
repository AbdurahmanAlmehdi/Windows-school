using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class InvoiceServiceTests
{
    [Fact]
    public void GenerateInvoice_ProducesCorrectArithmetic_PerFR_INV_3()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.ActualCheckOut = stay.CheckInDate.AddDays(3);

        var inv = h.Invoices.GenerateInvoice(stay);

        inv.Subtotal.Should().BeGreaterThan(0);
        inv.Tax.Should().Be(Math.Round(inv.Subtotal * 0.10m, 2));
        inv.Total.Should().Be(inv.Subtotal + inv.Tax);
    }

    [Fact]
    public void GenerateInvoice_AddsOneLinePerNight_PerFR_INV_1()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.ActualCheckOut = stay.CheckInDate.AddDays(3);

        var inv = h.Invoices.GenerateInvoice(stay);

        inv.Lines.Where(l => l.Category == InvoiceLineCategory.RoomCharge).Should().HaveCount(3);
    }

    [Fact]
    public void GenerateInvoice_ExcludesCancelledOrderLines_PerFR_INV_1()
    {
        // Use an Active stay with no seed orders so the test asserts on only
        // the orders it creates itself.
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStayWithoutOrders(h.Store);
        var item = h.Store.MenuItems[0];
        var live = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = item, Quantity = 1 } });
        var cancelled = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = item, Quantity = 9 } });
        h.Restaurant.CancelOrder(cancelled);
        stay.ActualCheckOut = stay.CheckInDate.AddDays(1);

        var inv = h.Invoices.GenerateInvoice(stay);

        inv.Lines
            .Where(l => l.Category == InvoiceLineCategory.RestaurantCharge)
            .Sum(l => l.Quantity)
            .Should().Be(1, "only the non-cancelled order's lines are billed");
    }

    [Fact]
    public void GenerateInvoice_AssignsAutoIncrementingInvoiceNumber_PerFR_INV_2()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.ActualCheckOut = stay.CheckInDate.AddDays(1);

        var a = h.Invoices.GenerateInvoice(stay);
        var b = h.Invoices.GenerateInvoice(stay);

        a.InvoiceNumber.Should().StartWith("INV-");
        b.InvoiceNumber.Should().StartWith("INV-");
        int.Parse(b.InvoiceNumber["INV-".Length..])
            .Should().BeGreaterThan(int.Parse(a.InvoiceNumber["INV-".Length..]));
    }

    [Fact]
    public void GenerateInvoice_DefaultsToPending_PerFR_INV_4()
    {
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);

        var inv = h.Invoices.GenerateInvoice(stay);

        inv.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void MarkPaid_TransitionsAndRecordsMethod_PerFR_INV_5()
    {
        var h = TestStoreFactory.Build();
        var inv = h.Store.Invoices.First(i => i.PaymentStatus == PaymentStatus.Pending);

        h.Invoices.MarkPaid(inv, PaymentMethod.Cash);

        inv.PaymentStatus.Should().Be(PaymentStatus.Paid);
        inv.PaymentMethod.Should().Be(PaymentMethod.Cash);
        inv.PaymentDate.Should().NotBeNull();
    }

    [Fact]
    public void MarkRefunded_TransitionsPaidToRefunded_PerFR_INV_6()
    {
        var h = TestStoreFactory.Build();
        var inv = h.Store.Invoices.First(i => i.PaymentStatus == PaymentStatus.Paid);

        h.Invoices.MarkRefunded(inv);

        inv.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void MarkRefunded_RejectsPendingInvoice_PerStateMachineSec3_5_3()
    {
        // SRS §3.5.3 invoice state machine: Pending -> Paid -> Refunded.
        // Refunding a Pending invoice is not a legal transition.
        var h = TestStoreFactory.Build();
        var inv = h.Store.Invoices.First(i => i.PaymentStatus == PaymentStatus.Pending);

        var act = () => h.Invoices.MarkRefunded(inv);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetUnpaidInvoices_ReturnsOnlyPending_PerFR_INV_7()
    {
        var h = TestStoreFactory.Build();

        var pending = h.Invoices.GetUnpaidInvoices();

        pending.Should().OnlyContain(i => i.PaymentStatus == PaymentStatus.Pending);
    }

    [Fact]
    public void GetOutstandingAmount_SumsPendingTotals_PerFR_INV_7()
    {
        var h = TestStoreFactory.Build();

        var expected = h.Store.Invoices
            .Where(i => i.PaymentStatus == PaymentStatus.Pending)
            .Sum(i => i.Total);

        h.Invoices.GetOutstandingAmount().Should().Be(expected);
    }

    [Fact]
    public void GetTotalRevenue_SumsPaidInvoices()
    {
        var h = TestStoreFactory.Build();

        var expected = h.Store.Invoices
            .Where(i => i.PaymentStatus == PaymentStatus.Paid)
            .Sum(i => i.Total);

        h.Invoices.GetTotalRevenue().Should().Be(expected);
    }

    [Fact]
    public void InvoiceTaxArithmetic_Is10Percent_PerCON_6()
    {
        var inv = new Invoice("INV-TEST");
        inv.Lines.Add(new InvoiceLine { Description = "X", Quantity = 1, UnitPrice = 100m, Category = InvoiceLineCategory.RoomCharge });

        inv.Subtotal.Should().Be(100m);
        inv.Tax.Should().Be(10m);
        inv.Total.Should().Be(110m);
    }

    [Fact]
    public void InvoiceTaxArithmetic_RoundsHalfAwayFromZero_PerNFR_REL_3()
    {
        var inv = new Invoice("INV-TEST");
        inv.Lines.Add(new InvoiceLine { Description = "X", Quantity = 1, UnitPrice = 7.95m, Category = InvoiceLineCategory.RoomCharge });

        inv.Tax.Should().Be(0.80m);
    }
}
