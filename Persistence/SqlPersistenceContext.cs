using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Persistence.Repositories;

namespace HotelManagement.WinForms.Persistence;

// Write-through implementation. Every Save/Delete opens a fresh
// SqlConnection (the underlying connection pool handles reuse),
// starts a transaction, calls the relevant repository, and commits.
//
// Atomicity is per call. A service method that mutates several
// aggregates issues several transactions; if one fails mid-way the
// earlier ones have already committed. For the academic build this
// is acceptable; a v1.3 enhancement would introduce a unit-of-work
// that batches all writes a service method makes into one
// transaction.
public sealed class SqlPersistenceContext : IPersistenceContext
{
    private readonly SqlDb _db;
    private readonly RoomRepository            _rooms;
    private readonly GuestRepository           _guests;
    private readonly MenuItemRepository        _menuItems;
    private readonly RoleRepository            _roles;
    private readonly UserRepository            _users;
    private readonly ReservationRepository     _reservations;
    private readonly StayRepository            _stays;
    private readonly RestaurantOrderRepository _orders;
    private readonly InvoiceRepository         _invoices;

    public SqlPersistenceContext(SqlDb db)
    {
        _db           = db;
        _rooms        = new RoomRepository(db);
        _guests       = new GuestRepository(db);
        _menuItems    = new MenuItemRepository(db);
        _roles        = new RoleRepository(db);
        _users        = new UserRepository(db);
        _reservations = new ReservationRepository(db);
        _stays        = new StayRepository(db);
        _orders       = new RestaurantOrderRepository(db);
        _invoices     = new InvoiceRepository(db);
    }

    public void SaveRoom(Room r)             => WithTx((c, tx) => _rooms.Upsert(r, c, tx));
    public void DeleteRoom(Room r)           => WithTx((c, tx) => _rooms.Delete(r, c, tx));
    public void SaveGuest(Guest g)           => WithTx((c, tx) => _guests.Upsert(g, c, tx));
    public void DeleteGuest(Guest g)         => WithTx((c, tx) => _guests.Delete(g, c, tx));
    public void SaveReservation(Reservation r)  => WithTx((c, tx) => _reservations.Upsert(r, c, tx));
    public void DeleteReservation(Reservation r) => WithTx((c, tx) => _reservations.Delete(r, c, tx));
    public void SaveStay(Stay s)             => WithTx((c, tx) => _stays.Upsert(s, c, tx));
    public void DeleteStay(Stay s)           => WithTx((c, tx) => _stays.Delete(s, c, tx));
    public void SaveOrder(RestaurantOrder o) => WithTx((c, tx) => _orders.Upsert(o, c, tx));
    public void DeleteOrder(RestaurantOrder o) => WithTx((c, tx) => _orders.Delete(o, c, tx));
    public void SaveInvoice(Invoice i)       => WithTx((c, tx) => _invoices.Upsert(i, c, tx));
    public void DeleteInvoice(Invoice i)     => WithTx((c, tx) => _invoices.Delete(i, c, tx));
    public void SaveMenuItem(MenuItem m)     => WithTx((c, tx) => _menuItems.Upsert(m, c, tx));
    public void DeleteMenuItem(MenuItem m)   => WithTx((c, tx) => _menuItems.Delete(m, c, tx));
    public void SaveUser(User u)             => WithTx((c, tx) => _users.Upsert(u, c, tx));
    public void DeleteUser(User u)           => WithTx((c, tx) => _users.Delete(u, c, tx));
    public void SaveRole(Role r)             => WithTx((c, tx) => _roles.Upsert(r, c, tx));
    public void DeleteRole(Role r)           => WithTx((c, tx) => _roles.Delete(r, c, tx));

    private void WithTx(Action<SqlConnection, SqlTransaction> work)
    {
        using var c  = _db.Open();
        using var tx = c.BeginTransaction();
        try
        {
            work(c, tx);
            tx.Commit();
        }
        catch
        {
            try { tx.Rollback(); } catch { /* swallow */ }
            throw;
        }
    }
}
