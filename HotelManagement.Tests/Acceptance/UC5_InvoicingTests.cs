using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Acceptance;

// =====================================================================
// UC-5: Generate and Pay the Guest Folio
// =====================================================================
//
// As a front-desk staff member at checkout time,
// I want to generate the guest folio (Invoice) and record payment,
// so that the stay is closed financially.
//
// Acceptance criteria covered:
//   - An invoice is generated from a stay's room nights and orders
//     (FR-INV-1, FR-INV-3).
//   - Tax is 10% of subtotal, rounded half-away-from-zero (CON-6, NFR-REL-3).
//   - A newly generated invoice starts as Pending (FR-INV-4).
//   - Marking the invoice paid records the method and timestamp (FR-INV-5).
//   - Cancelled-order lines never appear on the invoice (DEF-15).
//   - Sequential invoices receive ascending invoice numbers (FR-INV-2).
// =====================================================================

public class UC5_InvoicingTests
{
    [Fact]
    public void Scenario_StaffGeneratesFolioForCheckoutGuest_InvoiceContainsRoomAndOrderLines()
    {
        // Given an in-house stay with a backdated check-in so that nights > 1
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.ActualCheckOut = stay.CheckInDate.AddDays(3);

        // When the staff generates the folio
        var inv = h.Invoices.GenerateInvoice(stay);

        // Then the folio shows three room-night lines
        inv.Lines.Count(l => l.Category == InvoiceLineCategory.RoomCharge).Should().Be(3);

        // And the totals add up coherently
        inv.Subtotal.Should().BeGreaterThan(0);
        inv.Tax.Should().Be(Math.Round(inv.Subtotal * 0.10m, 2));
        inv.Total.Should().Be(inv.Subtotal + inv.Tax);
    }

    [Fact]
    public void Scenario_NewlyGeneratedFolio_DefaultsToPending_PerFR_INV_4()
    {
        // Given a stay ready for checkout
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);

        // When the folio is generated
        var inv = h.Invoices.GenerateInvoice(stay);

        // Then it sits in Pending until payment is taken
        inv.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Scenario_GuestPaysByCard_FolioBecomesPaidWithMethodAndDate_PerFR_INV_5()
    {
        // Given a Pending invoice
        var h = TestStoreFactory.Build();
        var inv = h.Store.Invoices.First(i => i.PaymentStatus == PaymentStatus.Pending);

        // When the staff records a credit-card payment
        h.Invoices.MarkPaid(inv, PaymentMethod.CreditCard);

        // Then the invoice transitions to Paid and the method + date are stored
        inv.PaymentStatus.Should().Be(PaymentStatus.Paid);
        inv.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
        inv.PaymentDate.Should().NotBeNull();
    }

    [Fact]
    public void Scenario_CancelledOrderLines_DoNotAppearOnGuestFolio_PerFR_INV_1()
    {
        // Given a clean active stay with one live order and one cancelled
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStayWithoutOrders(h.Store);
        var menuItem = h.Store.MenuItems[0];
        var live      = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = menuItem, Quantity = 1 } });
        var cancelled = h.Restaurant.CreateOrder(stay, new[] { new OrderLine { MenuItem = menuItem, Quantity = 9 } });
        h.Restaurant.CancelOrder(cancelled);
        stay.ActualCheckOut = stay.CheckInDate.AddDays(1);

        // When the folio is generated
        var inv = h.Invoices.GenerateInvoice(stay);

        // Then only the live order's units appear on the folio
        inv.Lines
            .Where(l => l.Category == InvoiceLineCategory.RestaurantCharge)
            .Sum(l => l.Quantity)
            .Should().Be(1);
    }

    [Fact]
    public void Scenario_TwoFoliosGeneratedBackToBack_InvoiceNumbersAreSequential_PerFR_INV_2()
    {
        // Given an in-house stay
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.ActualCheckOut = stay.CheckInDate.AddDays(1);

        // When the staff generates two folios for the stay back-to-back
        var first  = h.Invoices.GenerateInvoice(stay);
        var second = h.Invoices.GenerateInvoice(stay);

        // Then both numbers follow the INV-#### format and the second is higher
        first.InvoiceNumber.Should().StartWith("INV-");
        second.InvoiceNumber.Should().StartWith("INV-");
        int.Parse(second.InvoiceNumber["INV-".Length..])
            .Should().BeGreaterThan(int.Parse(first.InvoiceNumber["INV-".Length..]));
    }

    [Fact]
    public void Scenario_TaxArithmeticRoundsHalfAwayFromZero_PerNFR_REL_3()
    {
        // Given a synthesised invoice with a 7.95 subtotal (tax = 0.795 -> 0.80)
        var inv = new Invoice("INV-TEST");
        inv.Lines.Add(new InvoiceLine
        {
            Description = "Test", Quantity = 1, UnitPrice = 7.95m,
            Category = InvoiceLineCategory.RoomCharge
        });

        // Then the rounded tax is 0.80
        inv.Tax.Should().Be(0.80m);
    }
}
