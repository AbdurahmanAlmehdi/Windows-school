namespace HotelManagement.WinForms.Models;

public class Stay
{
    public Guest Guest { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public DateTime CheckInDate { get; set; }
    public DateTime ExpectedCheckOut { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public decimal RoomCharges { get; set; }
    public decimal RestaurantCharges { get; set; }
    public StayStatus Status { get; set; }

    public decimal TotalCharges => RoomCharges + RestaurantCharges;

    public string DisplayLabel => $"{Guest.Name} - Room {Room.Number}";
}
