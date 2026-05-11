namespace HotelManagement.WinForms.Models;

public class User
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Role Role { get; set; } = null!;

    public bool Can(PermissionResource resource, PermissionAction action) =>
        Role != null && Role.Has(resource, action);
}
