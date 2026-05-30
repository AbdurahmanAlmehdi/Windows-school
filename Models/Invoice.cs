using System.ComponentModel;

namespace HotelManagement.WinForms.Models;

public class Invoice
{
    private static int _nextNumber = 1001;

    public Guid Id { get; set; } = Guid.NewGuid();
    public string InvoiceNumber { get; set; }
    public Stay Stay { get; set; } = null!;
    public Guest Guest { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public BindingList<InvoiceLine> Lines { get; set; } = new();

    public decimal Subtotal => Lines.Sum(l => l.LineTotal);
    public decimal Tax => Math.Round(Subtotal * 0.10m, 2);
    public decimal Total => Subtotal + Tax;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }

    public Invoice()
    {
        InvoiceNumber = $"INV-{_nextNumber++}";
    }

    public Invoice(string invoiceNumber)
    {
        InvoiceNumber = invoiceNumber;
    }

    // Used after loading existing invoices from SQL Server so newly-created
    // invoices in the same session continue numbering from the right place.
    // (Replaces the static-counter hazard noted in StaticTestReport.md SC-04
    // until the codebase fully migrates to the dbo.seq_invoice_number
    // sequence on the SQL side.)
    public static void AdvanceSequenceTo(int next)
    {
        if (next > _nextNumber) _nextNumber = next;
    }
}
