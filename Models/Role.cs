namespace HotelManagement.WinForms.Models;

public class Role
{
    public string Name { get; set; } = string.Empty;
    public HashSet<Permission> Permissions { get; set; } = new();

    // System roles (e.g. SuperAdmin) cannot be edited, renamed, or removed via the UI.
    public bool IsSystem { get; set; }

    public bool Has(PermissionResource resource, PermissionAction action) =>
        Permissions.Contains(new Permission(resource, action));

    public override string ToString() => Name;
}
