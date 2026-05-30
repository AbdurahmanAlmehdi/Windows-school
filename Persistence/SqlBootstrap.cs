using System.Text;
using Microsoft.Data.SqlClient;

namespace HotelManagement.WinForms.Persistence;

// Handles first-launch wiring against SQL Server:
//   1. Ensure the HotelManagement database exists (creates it on master if missing).
//   2. If no tables exist yet, run db/schema_sqlserver.sql.
//   3. If the rooms table is empty, run db/seed_sqlserver.sql.
//
// Script execution is GO-aware: we split each .sql file on lines that
// consist only of "GO" and submit each batch with its own SqlCommand,
// because Microsoft.Data.SqlClient cannot execute multi-batch text.
public sealed class SqlBootstrap
{
    private readonly AppConfig _config;

    public SqlBootstrap(AppConfig config)
    {
        _config = config;
    }

    public void EnsureReady()
    {
        if (!_config.IsSqlServer) return;

        EnsureDatabaseExists();

        if (!SchemaExists())
            RunScript("schema_sqlserver.sql");

        if (RoomsTableEmpty())
            RunScript("seed_sqlserver.sql");
    }

    private void EnsureDatabaseExists()
    {
        if (string.IsNullOrWhiteSpace(_config.MasterConnectionString))
            return;

        const string sql = @"
            IF DB_ID(N'HotelManagement') IS NULL
                EXEC('CREATE DATABASE [HotelManagement]');";

        using var c = new SqlConnection(_config.MasterConnectionString);
        c.Open();
        using var cmd = new SqlCommand(sql, c);
        cmd.ExecuteNonQuery();
    }

    private bool SchemaExists()
    {
        const string sql = "SELECT OBJECT_ID(N'dbo.rooms', N'U');";
        using var c = new SqlConnection(_config.ConnectionString);
        c.Open();
        using var cmd = new SqlCommand(sql, c);
        var result = cmd.ExecuteScalar();
        return result != null && result != DBNull.Value;
    }

    private bool RoomsTableEmpty()
    {
        const string sql = "SELECT COUNT(1) FROM dbo.rooms;";
        using var c = new SqlConnection(_config.ConnectionString);
        c.Open();
        using var cmd = new SqlCommand(sql, c);
        var count = Convert.ToInt32(cmd.ExecuteScalar());
        return count == 0;
    }

    private void RunScript(string filename)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "db", filename);
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"SQL script not found: {path}. " +
                "Verify that the db\\ folder is copied to the output directory.",
                path);

        var raw = File.ReadAllText(path);
        var batches = SplitOnGoBatches(raw);

        using var c = new SqlConnection(_config.ConnectionString);
        c.Open();

        foreach (var batch in batches)
        {
            using var cmd = new SqlCommand(batch, c) { CommandTimeout = 120 };
            cmd.ExecuteNonQuery();
        }
    }

    // Split a T-SQL script on lines whose only non-whitespace content is "GO".
    // Skips empty batches.
    private static List<string> SplitOnGoBatches(string script)
    {
        var batches = new List<string>();
        var current = new StringBuilder();

        foreach (var raw in script.Split('\n'))
        {
            var trimmed = raw.TrimEnd('\r').Trim();
            if (string.Equals(trimmed, "GO", StringComparison.OrdinalIgnoreCase))
            {
                var batch = current.ToString().Trim();
                if (batch.Length > 0)
                    batches.Add(batch);
                current.Clear();
                continue;
            }
            current.AppendLine(raw.TrimEnd('\r'));
        }

        var tail = current.ToString().Trim();
        if (tail.Length > 0)
            batches.Add(tail);

        return batches;
    }
}
