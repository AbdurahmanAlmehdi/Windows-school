namespace HotelManagement.WinForms.Models;

public class Reservation
{
    public Guest Guest { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public ReservationStatus Status { get; set; }
    public List<AccompanyingGuest> Accompanying { get; set; } = new();
    public string? MarriageCertificatePath { get; set; }

    public int AdultCount => 1 + Accompanying.Count(a => !a.IsChild);
    public int ChildCount => Accompanying.Count(a => a.IsChild);
    public int PartySize => 1 + Accompanying.Count;

    // Children count as half an adult for capacity math.
    public int CapacityUnits => 2 * AdultCount + ChildCount;
}
