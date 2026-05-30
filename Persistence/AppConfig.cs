using Microsoft.Extensions.Configuration;

namespace HotelManagement.WinForms.Persistence;

public enum PersistenceMode
{
    InMemory,
    SqlServer
}

public sealed class AppConfig
{
    public PersistenceMode Mode { get; }
    public string ConnectionString { get; }
    public string MasterConnectionString { get; }

    public bool IsSqlServer => Mode == PersistenceMode.SqlServer;

    private AppConfig(PersistenceMode mode, string conn, string master)
    {
        Mode = mode;
        ConnectionString = conn;
        MasterConnectionString = master;
    }

    public static AppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            return new AppConfig(PersistenceMode.InMemory, "", "");

        var cfg = new ConfigurationBuilder()
            .AddJsonFile(path, optional: true, reloadOnChange: false)
            .Build();

        var modeText = cfg["Persistence:Mode"] ?? "InMemory";
        var mode = string.Equals(modeText, "SqlServer", StringComparison.OrdinalIgnoreCase)
            ? PersistenceMode.SqlServer
            : PersistenceMode.InMemory;

        var conn   = cfg.GetConnectionString("HotelManagement") ?? "";
        var master = cfg["ConnectionStrings:_master"] ?? "";

        return new AppConfig(mode, conn, master);
    }
}
