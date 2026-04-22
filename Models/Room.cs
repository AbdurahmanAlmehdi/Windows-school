namespace HotelManagement.WinForms.Models;

public class Room
{
    public int Number { get; set; }
    public RoomType Type { get; set; }
    public decimal Rate { get; set; }
    public bool IsOccupied { get; set; }
    public RoomCondition Condition { get; set; } = RoomCondition.Clean;
    public string MaintenanceLog { get; set; } = string.Empty;

    public bool IsAvailable => !IsOccupied && Condition == RoomCondition.Clean;

    public string DisplayStatus
    {
        get
        {
            if (Condition == RoomCondition.OutOfService)
                return "Out of Service";
            if (IsOccupied && Condition == RoomCondition.NeedsCleaning)
                return "Occupied + Needs Cleaning";
            if (IsOccupied)
                return "Occupied";
            if (Condition == RoomCondition.NeedsCleaning)
                return "Needs Cleaning";
            return "Available";
        }
    }

    public override string ToString() => $"Room {Number} ({Type}) - ${Rate}/night";
}
