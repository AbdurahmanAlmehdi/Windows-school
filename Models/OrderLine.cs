namespace HotelManagement.WinForms.Models;

public class OrderLine
{
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    public string Notes { get; set; } = string.Empty;

    public decimal LineTotal => MenuItem?.Price * Quantity ?? 0;

    public override string ToString() => $"{MenuItem.Name} x{Quantity} = ${LineTotal:F2}";
}
