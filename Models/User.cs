namespace HotelManagement.WinForms.Models;

public class User
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsManager => Role == UserRole.Manager;
}
