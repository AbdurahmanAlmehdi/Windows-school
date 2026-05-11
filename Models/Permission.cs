namespace HotelManagement.WinForms.Models;

public enum PermissionResource
{
    Rooms,
    Reservations,
    MenuItems,
    Orders,
    Invoices,
    Users
}

public enum PermissionAction
{
    Create,
    Read,
    Update,
    Delete
}

public readonly record struct Permission(PermissionResource Resource, PermissionAction Action)
{
    public override string ToString() => $"{Resource}.{Action}";

    public static IEnumerable<Permission> All()
    {
        foreach (var r in Enum.GetValues<PermissionResource>())
            foreach (var a in Enum.GetValues<PermissionAction>())
                yield return new Permission(r, a);
    }
}
