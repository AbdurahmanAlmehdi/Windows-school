using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Services;
using HotelManagement.WinForms.Forms;

namespace HotelManagement.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var dataStore = new DataStore();
        var authService = new AuthService(dataStore);
        var roomService = new RoomService(dataStore);
        var bookingService = new BookingService(dataStore, roomService);
        var restaurantService = new RestaurantService(dataStore);
        var reportService = new ReportService(dataStore);
        var invoiceService = new InvoiceService(dataStore);

        using var loginForm = new LoginForm(authService);
        Application.Run(loginForm);

        if (authService.CurrentUser != null)
        {
            Application.Run(new MainForm(
                authService, roomService, bookingService,
                restaurantService, reportService, invoiceService, dataStore));
        }
    }
}
