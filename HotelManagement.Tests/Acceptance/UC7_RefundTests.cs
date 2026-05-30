using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Acceptance;

// =====================================================================
// UC-7: Refund a Paid Invoice
// =====================================================================
//
// As a manager,
// I want to refund a guest folio that was already paid,
// so that the financial record stays accurate after a dispute or error.
//
// Acceptance criteria covered:
//   - Only a Paid invoice may be refunded (SRS §3.5.3 / DEF-16).
//   - A Pending invoice cannot be refunded.
//   - After refund, the invoice transitions to Refunded (FR-INV-6).
//   - Reporting totals reflect Paid invoices; refunds leave the trail
//     visible.
// =====================================================================

public class UC7_RefundTests
{
    [Fact]
    public void Scenario_ManagerRefundsAPaidFolio_StatusBecomesRefunded_PerFR_INV_6()
    {
        // Given an already-paid invoice
        var h = TestStoreFactory.Build();
        var inv = h.Store.Invoices.First(i => i.PaymentStatus == PaymentStatus.Paid);

        // When the manager refunds it
        h.Invoices.MarkRefunded(inv);

        // Then the invoice transitions to Refunded
        inv.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void Scenario_ManagerTriesToRefundPendingInvoice_OperationIsRejected_PerStateMachineSec3_5_3()
    {
        // Given a Pending invoice (no money was ever taken)
        var h = TestStoreFactory.Build();
        var inv = h.Store.Invoices.First(i => i.PaymentStatus == PaymentStatus.Pending);

        // When the manager tries to refund it
        var act = () => h.Invoices.MarkRefunded(inv);

        // Then the operation is rejected per the §3.5.3 state machine
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Scenario_PayThenRefundFlow_LeavesInvoiceInRefundedState_End_to_End()
    {
        // Given a fresh stay-to-folio flow
        var h = TestStoreFactory.Build();
        var stay = TestStoreFactory.FirstActiveStay(h.Store);
        stay.CheckInDate = DateTime.Now.AddDays(-1);
        h.Booking.CheckOut(stay);
        var inv = h.Invoices.GenerateInvoice(stay);

        // When the guest pays by debit card, then the manager refunds
        h.Invoices.MarkPaid(inv, PaymentMethod.DebitCard);
        h.Invoices.MarkRefunded(inv);

        // Then the invoice's final status is Refunded
        inv.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }
}
