namespace HotelManagement.WinForms.Models;

public class AccompanyingGuest
{
    public string Name { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.Male;
    public int Age { get; set; }
    public string Passport { get; set; } = string.Empty;

    public bool IsChild => Age > 0 && Age < 18;
}
