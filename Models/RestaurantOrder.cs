using System.ComponentModel;

namespace HotelManagement.WinForms.Models;

public class RestaurantOrder
{
    public Stay Stay { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public BindingList<OrderLine> Lines { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public decimal Total => Lines.Sum(l => l.LineTotal);
    public int ItemCount => Lines.Sum(l => l.Quantity);
}
