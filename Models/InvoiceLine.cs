namespace HotelManagement.WinForms.Models;

public class InvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public InvoiceLineCategory Category { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}
