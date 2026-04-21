namespace HotelManagement.WinForms.Models;

public class Reservation
{
    public Guest Guest { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public ReservationStatus Status { get; set; }
}
