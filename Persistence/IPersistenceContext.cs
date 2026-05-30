using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence;

// Write-through persistence boundary the service layer talks to.
// Each mutation is its own atomic operation on SQL Server (the
// SqlPersistenceContext implementation opens a transaction, applies
// the change, and commits).
//
// Tests and InMemory mode use NullPersistenceContext.Instance which
// no-ops every call, so service-layer code stays identical across
// modes.
public interface IPersistenceContext
{
    void SaveRoom(Room room);
    void DeleteRoom(Room room);

    void SaveGuest(Guest guest);
    void DeleteGuest(Guest guest);

    void SaveReservation(Reservation reservation);
    void DeleteReservation(Reservation reservation);

    void SaveStay(Stay stay);
    void DeleteStay(Stay stay);

    void SaveOrder(RestaurantOrder order);
    void DeleteOrder(RestaurantOrder order);

    void SaveInvoice(Invoice invoice);
    void DeleteInvoice(Invoice invoice);

    void SaveMenuItem(MenuItem item);
    void DeleteMenuItem(MenuItem item);

    void SaveUser(User user);
    void DeleteUser(User user);

    void SaveRole(Role role);
    void DeleteRole(Role role);
}
