using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence.Repositories;

// Owns both `roles` and the child `role_permissions` table.
public sealed class RoleRepository
{
    private readonly SqlDb _db;
    public RoleRepository(SqlDb db) { _db = db; }

    private const string SelectRoles = @"
        SELECT role_id, name, is_system
        FROM   dbo.roles
        ORDER  BY name;";

    private const string SelectPermissions = @"
        SELECT role_id, resource, [action]
        FROM   dbo.role_permissions;";

    private const string DeletePermissions = @"DELETE FROM dbo.role_permissions;";
    private const string DeleteRoles       = @"DELETE FROM dbo.roles;";

    private const string InsertRole = @"
        INSERT INTO dbo.roles (role_id, name, is_system)
        VALUES (@id, @name, @sys);";

    private const string InsertPermission = @"
        INSERT INTO dbo.role_permissions (role_id, resource, [action])
        VALUES (@role, @resource, @action);";

    public List<Role> GetAll()
    {
        using var c = _db.Open();

        // Step 1: load roles.
        var roles = new List<Role>();
        var byId = new Dictionary<Guid, Role>();

        using (var cmd = new SqlCommand(SelectRoles, c))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var role = new Role
                {
                    Id       = r.GetGuid(0),
                    Name     = r.GetString(1),
                    IsSystem = r.GetBoolean(2),
                    Permissions = new HashSet<Permission>()
                };
                roles.Add(role);
                byId[role.Id] = role;
            }
        }

        // Step 2: load permissions and attach.
        using (var cmd = new SqlCommand(SelectPermissions, c))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var roleId   = r.GetGuid(0);
                var resource = Enum.Parse<PermissionResource>(r.GetString(1));
                var action   = Enum.Parse<PermissionAction>(r.GetString(2));
                if (byId.TryGetValue(roleId, out var role))
                    role.Permissions.Add(new Permission(resource, action));
            }
        }

        return roles;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        // Child first because of FK ordering, even though role_permissions
        // cascades on role delete - explicit is clearer.
        using (var cmd = new SqlCommand(DeletePermissions, c, tx)) cmd.ExecuteNonQuery();
        using (var cmd = new SqlCommand(DeleteRoles, c, tx))       cmd.ExecuteNonQuery();
    }

    public void Insert(Role role, SqlConnection c, SqlTransaction tx)
    {
        using (var cmd = new SqlCommand(InsertRole, c, tx))
        {
            cmd.Parameters.AddWithValue("@id",   role.Id);
            cmd.Parameters.AddWithValue("@name", role.Name);
            cmd.Parameters.AddWithValue("@sys",  role.IsSystem);
            cmd.ExecuteNonQuery();
        }

        foreach (var p in role.Permissions)
        {
            using var cmd = new SqlCommand(InsertPermission, c, tx);
            cmd.Parameters.AddWithValue("@role",     role.Id);
            cmd.Parameters.AddWithValue("@resource", p.Resource.ToString());
            cmd.Parameters.AddWithValue("@action",   p.Action.ToString());
            cmd.ExecuteNonQuery();
        }
    }
}
