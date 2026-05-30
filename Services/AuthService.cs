using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services.Security;

namespace HotelManagement.WinForms.Services;

public class AuthService
{
    private readonly DataStore _store;

    public AuthService(DataStore store)
    {
        _store = store;
    }

    public User? CurrentUser { get; private set; }

    public bool Login(string? username, string? password)
    {
        if (username == null || password == null) return false;

        var user = _store.Users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user != null && PasswordHasher.Verify(password, user.Password))
        {
            CurrentUser = user;
            return true;
        }
        return false;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public bool Can(PermissionResource resource, PermissionAction action) =>
        CurrentUser?.Can(resource, action) == true;

    public void Require(PermissionResource resource, PermissionAction action)
    {
        if (!Can(resource, action))
            throw new UnauthorizedAccessException(
                $"Your role does not have permission to {action} on {resource}.");
    }
}
