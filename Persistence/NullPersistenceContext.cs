using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence;

// No-op persistence context used by:
//   - The unit test project (services mutate DataStore only).
//   - InMemory mode in production (no SQL Server configured).
//
// Singleton because it carries no state; passing the same instance
// everywhere avoids needless allocations during DataStore churn.
public sealed class NullPersistenceContext : IPersistenceContext
{
    public static readonly NullPersistenceContext Instance = new();

    private NullPersistenceContext() { }

    public void SaveRoom(Room _) { }
    public void DeleteRoom(Room _) { }
    public void SaveGuest(Guest _) { }
    public void DeleteGuest(Guest _) { }
    public void SaveReservation(Reservation _) { }
    public void DeleteReservation(Reservation _) { }
    public void SaveStay(Stay _) { }
    public void DeleteStay(Stay _) { }
    public void SaveOrder(RestaurantOrder _) { }
    public void DeleteOrder(RestaurantOrder _) { }
    public void SaveInvoice(Invoice _) { }
    public void DeleteInvoice(Invoice _) { }
    public void SaveMenuItem(MenuItem _) { }
    public void DeleteMenuItem(MenuItem _) { }
    public void SaveUser(User _) { }
    public void DeleteUser(User _) { }
    public void SaveRole(Role _) { }
    public void DeleteRole(Role _) { }
}
