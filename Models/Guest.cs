namespace HotelManagement.WinForms.Models;

public class Guest
{
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Passport { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.Male;
    public bool IsVip { get; set; }
    public int StayCount { get; set; }

    public string DisplayLabel => $"{Name} ({Contact})";
}
