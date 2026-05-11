using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Services;

public class UserService
{
    private readonly DataStore _store;
    private readonly AuthService _auth;

    public UserService(DataStore store, AuthService auth)
    {
        _store = store;
        _auth = auth;
    }

    // --- Users ---

    public IEnumerable<User> GetUsers() => _store.Users;

    public User AddUser(string username, string password, Role role)
    {
        _auth.Require(PermissionResource.Users, PermissionAction.Create);

        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("Username is required.");
        if (string.IsNullOrEmpty(password))
            throw new InvalidOperationException("Password is required.");
        if (_store.Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"User '{username}' already exists.");
        if (role == null)
            throw new InvalidOperationException("Role is required.");

        var user = new User { Username = username.Trim(), Password = password, Role = role };
        _store.Users.Add(user);
        return user;
    }

    public void UpdateUser(User user, string username, string? newPassword, Role role)
    {
        _auth.Require(PermissionResource.Users, PermissionAction.Update);

        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("Username is required.");
        if (_store.Users.Any(u => u != user && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"User '{username}' already exists.");
        if (role == null)
            throw new InvalidOperationException("Role is required.");

        user.Username = username.Trim();
        if (!string.IsNullOrEmpty(newPassword))
            user.Password = newPassword;
        user.Role = role;
    }

    public void RemoveUser(User user)
    {
        _auth.Require(PermissionResource.Users, PermissionAction.Delete);

        if (user == _auth.CurrentUser)
            throw new InvalidOperationException("You cannot remove the currently signed-in user.");
        if (user.Role?.IsSystem == true && _store.Users.Count(u => u.Role?.IsSystem == true) <= 1)
            throw new InvalidOperationException("Cannot remove the last system administrator.");

        _store.Users.Remove(user);
    }

    // --- Roles ---

    public IEnumerable<Role> GetRoles() => _store.Roles;

    public Role AddRole(string name, IEnumerable<Permission> permissions)
    {
        _auth.Require(PermissionResource.Users, PermissionAction.Create);

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Role name is required.");
        if (_store.Roles.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Role '{name}' already exists.");

        var role = new Role { Name = name.Trim(), Permissions = permissions.ToHashSet() };
        _store.Roles.Add(role);
        return role;
    }

    public void UpdateRole(Role role, string name, IEnumerable<Permission> permissions)
    {
        _auth.Require(PermissionResource.Users, PermissionAction.Update);

        if (role.IsSystem)
            throw new InvalidOperationException($"The system role '{role.Name}' cannot be modified.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Role name is required.");
        if (_store.Roles.Any(r => r != role && r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Role '{name}' already exists.");

        role.Name = name.Trim();
        role.Permissions = permissions.ToHashSet();
    }

    public void RemoveRole(Role role)
    {
        _auth.Require(PermissionResource.Users, PermissionAction.Delete);

        if (role.IsSystem)
            throw new InvalidOperationException($"The system role '{role.Name}' cannot be removed.");
        if (_store.Users.Any(u => u.Role == role))
            throw new InvalidOperationException(
                $"Role '{role.Name}' is assigned to one or more users. Reassign them first.");

        _store.Roles.Remove(role);
    }
}
