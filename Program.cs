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

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var config = AppConfig.Load();
        var dataStore = new DataStore();
        PersistenceManager? persistence = null;

        if (config.IsSqlServer)
        {
            try
            {
                var bootstrap = new SqlBootstrap(config);
                bootstrap.EnsureReady();

                var db = new SqlDb(config.ConnectionString);
                persistence = new PersistenceManager(db);
                persistence.LoadInto(dataStore);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not connect to SQL Server. Running in in-memory mode.\n\n{ex.Message}",
                    "SQL Server unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                persistence = null;
            }
        }

        var authService = new AuthService(dataStore);
        var roomService = new RoomService(dataStore, authService);
        var bookingService = new BookingService(dataStore, roomService);
        var restaurantService = new RestaurantService(dataStore, authService);
        var reportService = new ReportService(dataStore);
        var invoiceService = new InvoiceService(dataStore);
        var userService = new UserService(dataStore, authService);

        using var loginForm = new LoginForm(authService);
        Application.Run(loginForm);

        if (authService.CurrentUser != null)
        {
            Application.Run(new MainForm(
                authService, roomService, bookingService,
                restaurantService, reportService, invoiceService,
                userService, dataStore, persistence));
        }
    }
}
