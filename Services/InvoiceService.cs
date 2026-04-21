using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Services;

public class InvoiceService
{
    private readonly DataStore _store;

    public InvoiceService(DataStore store)
    {
        _store = store;
    }

    public Invoice GenerateInvoice(Stay stay)
    {
        var invoice = new Invoice
        {
            Stay = stay,
            Guest = stay.Guest,
            Room = stay.Room,
            InvoiceDate = DateTime.Now
        };

        // Room night lines
        var checkOut = stay.ActualCheckOut ?? DateTime.Now;
        var nights = Math.Max(1, (int)(checkOut - stay.CheckInDate).TotalDays);
        for (int i = 0; i < nights; i++)
        {
            var nightDate = stay.CheckInDate.AddDays(i);
            invoice.Lines.Add(new InvoiceLine
            {
                Description = $"Room {stay.Room.Number} - Night {i + 1} ({nightDate:MMM dd})",
                Quantity = 1,
                UnitPrice = stay.Room.Rate,
                Category = InvoiceLineCategory.RoomCharge
            });
        }

        // Restaurant order lines
        var orders = _store.Orders.Where(o => o.Stay == stay && o.Status != OrderStatus.Cancelled);
        foreach (var order in orders)
        {
            foreach (var line in order.Lines)
            {
                invoice.Lines.Add(new InvoiceLine
                {
                    Description = line.MenuItem.Name,
                    Quantity = line.Quantity,
                    UnitPrice = line.MenuItem.Price,
                    Category = InvoiceLineCategory.RestaurantCharge
                });
            }
        }

        _store.Invoices.Add(invoice);
        return invoice;
    }

    public void MarkPaid(Invoice invoice, PaymentMethod method)
    {
        invoice.PaymentStatus = PaymentStatus.Paid;
        invoice.PaymentMethod = method;
        invoice.PaymentDate = DateTime.Now;
    }

    public void MarkRefunded(Invoice invoice)
    {
        invoice.PaymentStatus = PaymentStatus.Refunded;
    }

    public decimal GetTotalRevenue()
    {
        return _store.Invoices
            .Where(i => i.PaymentStatus == PaymentStatus.Paid)
            .Sum(i => i.Total);
    }

    public decimal GetTodayRevenue()
    {
        return _store.Invoices
            .Where(i => i.PaymentStatus == PaymentStatus.Paid && i.PaymentDate?.Date == DateTime.Today)
            .Sum(i => i.Total);
    }

    public decimal GetOutstandingAmount()
    {
        return _store.Invoices
            .Where(i => i.PaymentStatus == PaymentStatus.Pending)
            .Sum(i => i.Total);
    }

    public List<Invoice> GetUnpaidInvoices()
    {
        return _store.Invoices
            .Where(i => i.PaymentStatus == PaymentStatus.Pending)
            .ToList();
    }
}
