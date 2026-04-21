using System.ComponentModel;

namespace HotelManagement.WinForms.Models;

public class Invoice
{
    private static int _nextNumber = 1001;

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
}
