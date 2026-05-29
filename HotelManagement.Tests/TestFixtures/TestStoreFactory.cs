using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services;

namespace HotelManagement.Tests.TestFixtures;


internal static class TestStoreFactory
{
    public sealed class Harness
    {
        public DataStore Store { get; init; } = null!;
        public AuthService Auth { get; init; } = null!;
        public RoomService Rooms { get; init; } = null!;
        public BookingService Booking { get; init; } = null!;
        public RestaurantService Restaurant { get; init; } = null!;
        public InvoiceService Invoices { get; init; } = null!;
        public ReportService Reports { get; init; } = null!;
        public UserService Users { get; init; } = null!;
    }

    public static Harness Build(string? loginAs = "superadmin", string? password = "superadmin123")
    {
        var store = new DataStore();
        var auth = new AuthService(store);
        var rooms = new RoomService(store, auth);
        var booking = new BookingService(store, rooms);
        var restaurant = new RestaurantService(store, auth);
        var invoices = new InvoiceService(store);
        var reports = new ReportService(store);
        var users = new UserService(store, auth);

        if (loginAs != null)
            auth.Login(loginAs, password ?? string.Empty);

        return new Harness
        {
            Store = store,
            Auth = auth,
            Rooms = rooms,
            Booking = booking,
            Restaurant = restaurant,
            Invoices = invoices,
            Reports = reports,
            Users = users
        };
    }

    public static Room FirstAvailableRoom(DataStore store) =>
        store.Rooms.First(r => r.IsAvailable);

    public static Stay FirstActiveStay(DataStore store) =>
        store.Stays.First(s => s.Status == StayStatus.Active);

    public static Guest FirstGuest(DataStore store) => store.Guests[0];
}
