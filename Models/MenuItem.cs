namespace HotelManagement.WinForms.Models;

public class MenuItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string Description { get; set; } = string.Empty;

    public override string ToString() => $"{Name} - ${Price:F2}";
}
