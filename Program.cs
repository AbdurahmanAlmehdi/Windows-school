using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Forms;
using HotelManagement.WinForms.Persistence;
using HotelManagement.WinForms.Services;

namespace HotelManagement.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // QuestPDF is licensed Community for this build (academic / non-commercial).
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var config = AppConfig.Load();
        var dataStore = new DataStore();
        IPersistenceContext persistence = NullPersistenceContext.Instance;

        if (config.IsSqlServer)
        {
            try
            {
                var bootstrap = new SqlBootstrap(config);
                bootstrap.EnsureReady();

                var db = new SqlDb(config.ConnectionString);

                // Initial load: replace the in-memory seed with what SQL holds.
                var loader = new PersistenceManager(db);
                loader.LoadInto(dataStore);

                // Subsequent mutations write through this context, so every
                // service-layer change persists to SQL Server immediately.
                persistence = new SqlPersistenceContext(db);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not connect to SQL Server. Running in in-memory mode.\n\n{ex.Message}",
                    "SQL Server unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                persistence = NullPersistenceContext.Instance;
            }
        }

        var authService       = new AuthService(dataStore);
        var roomService       = new RoomService(dataStore, authService, persistence);
        var bookingService    = new BookingService(dataStore, roomService, persistence);
        var restaurantService = new RestaurantService(dataStore, authService, persistence);
        var reportService     = new ReportService(dataStore);
        var invoiceService    = new InvoiceService(dataStore, persistence);
        var userService       = new UserService(dataStore, authService, persistence);

        using var loginForm = new LoginForm(authService);
        Application.Run(loginForm);

        if (authService.CurrentUser != null)
        {
            Application.Run(new MainForm(
                authService, roomService, bookingService,
                restaurantService, reportService, invoiceService,
                userService, dataStore));
        }
    }
}
