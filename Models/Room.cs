namespace HotelManagement.WinForms.Models;

public class Room
{
    public int Number { get; set; }
    public RoomType Type { get; set; }
    public decimal Rate { get; set; }
    public RoomStatus Status { get; set; }
    public string MaintenanceLog { get; set; } = string.Empty;

    public override string ToString() => $"Room {Number} ({Type}) - ${Rate}/night";
}
