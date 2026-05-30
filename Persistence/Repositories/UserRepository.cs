using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services.Security;

namespace HotelManagement.WinForms.Persistence.Repositories;

public sealed class UserRepository
{
    private readonly SqlDb _db;
    public UserRepository(SqlDb db) { _db = db; }

    private const string SelectAll = @"
        SELECT user_id, username, password_hash, role_id
        FROM   dbo.users
        ORDER  BY username;";

    private const string DeleteAllSql = @"DELETE FROM dbo.users;";

    private const string InsertSql = @"
        INSERT INTO dbo.users (user_id, username, password_hash, role_id)
        VALUES (@id, @username, @password, @role);";

    public List<User> GetAll(IReadOnlyDictionary<Guid, Role> rolesById)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand(SelectAll, c);
        using var r = cmd.ExecuteReader();

        var users = new List<User>();
        while (r.Read())
        {
            var roleId = r.GetGuid(3);
            if (!rolesById.TryGetValue(roleId, out var role))
                throw new InvalidDataException(
                    $"User '{r.GetString(1)}' references unknown role {roleId}. " +
                    "Load roles before users.");


            var stored = r.GetString(2);
            var passwordHash = PasswordHasher.IsHash(stored) ? stored : PasswordHasher.Hash(stored);

            users.Add(new User
            {
                Id       = r.GetGuid(0),
                Username = r.GetString(1),
                Password = passwordHash,
                Role     = role
            });
        }
        return users;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(DeleteAllSql, c, tx);
        cmd.ExecuteNonQuery();
    }

    public void Insert(User user, SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(InsertSql, c, tx);
        cmd.Parameters.AddWithValue("@id",       user.Id);
        cmd.Parameters.AddWithValue("@username", user.Username);
        // Password field is intentionally written as-is. Production deployments
        // must replace this with a hashed value per NFR-SEC-1; the schema column
        // is named password_hash for that future migration.
        cmd.Parameters.AddWithValue("@password", user.Password);
        cmd.Parameters.AddWithValue("@role",     user.Role.Id);
        cmd.ExecuteNonQuery();
    }
}
