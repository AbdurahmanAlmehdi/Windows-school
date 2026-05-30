using Microsoft.Data.SqlClient;

namespace HotelManagement.WinForms.Persistence;

// Thin wrapper around SqlConnection that the rest of the persistence
// layer composes around. Every SQL statement is parameterised; no
// caller ever concatenates user input into a command string.
public sealed class SqlDb
{
    private readonly string _connectionString;

    public SqlDb(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is empty.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public SqlConnection Open()
    {
        var c = new SqlConnection(_connectionString);
        c.Open();
        return c;
    }

    // Convenience helper for fire-and-forget non-query statements
    // (used by the bootstrapper). Uses a fresh connection so it does
    // not interact with any open transactions.
    public int ExecuteNonQuery(string sql)
    {
        using var c = Open();
        using var cmd = new SqlCommand(sql, c) { CommandTimeout = 60 };
        return cmd.ExecuteNonQuery();
    }
}
