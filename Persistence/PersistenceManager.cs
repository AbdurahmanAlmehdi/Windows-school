using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Persistence.Repositories;

namespace HotelManagement.WinForms.Persistence;


public sealed class PersistenceManager
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

    public PersistenceManager(SqlDb db)
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

    public void LoadInto(DataStore store)
    {

        var rooms      = _rooms.GetAll();
        var guests     = _guests.GetAll();
        var menuItems  = _menuItems.GetAll();
        var roles      = _roles.GetAll();

        var roomsById = rooms.ToDictionary(r => r.Id);
        var guestsById = guests.ToDictionary(g => g.Id);
        var itemsById = menuItems.ToDictionary(m => m.Id);
        var rolesById = roles.ToDictionary(r => r.Id);


        var users        = _users.GetAll(rolesById);
        var reservations = _reservations.GetAll(guestsById, roomsById);
        var stays        = _stays.GetAll(guestsById, roomsById);
        var staysById    = stays.ToDictionary(s => s.Id);

        var orders   = _orders.GetAll(staysById, itemsById);
        var invoices = _invoices.GetAll(staysById, guestsById, roomsById);

        Replace(store.Rooms,        rooms);
        Replace(store.Guests,       guests);
        Replace(store.MenuItems,    menuItems);
        Replace(store.Roles,        roles);
        Replace(store.Users,        users);
        Replace(store.Reservations, reservations);
        Replace(store.Stays,        stays);
        Replace(store.Orders,       orders);
        Replace(store.Invoices,     invoices);
    }

    public void SaveFrom(DataStore store)
    {
        using var c  = _db.Open();
        using var tx = c.BeginTransaction();
        try
        {

            _invoices    .DeleteAll(c, tx);
            _orders      .DeleteAll(c, tx);
            _stays       .DeleteAll(c, tx);
            _reservations.DeleteAll(c, tx);
            _users       .DeleteAll(c, tx);
            _roles       .DeleteAll(c, tx);
            _menuItems   .DeleteAll(c, tx);
            _guests      .DeleteAll(c, tx);
            _rooms       .DeleteAll(c, tx);

            // Insert in dependency order.
            foreach (var room  in store.Rooms)     _rooms    .Insert(room,  c, tx);
            foreach (var guest in store.Guests)    _guests   .Insert(guest, c, tx);
            foreach (var item  in store.MenuItems) _menuItems.Insert(item,  c, tx);
            foreach (var role  in store.Roles)     _roles    .Insert(role,  c, tx);
            foreach (var user  in store.Users)     _users    .Insert(user,  c, tx);
            foreach (var res   in store.Reservations) _reservations.Insert(res,   c, tx);
            foreach (var stay  in store.Stays)        _stays       .Insert(stay,  c, tx);
            foreach (var order in store.Orders)       _orders      .Insert(order, c, tx);
            foreach (var inv   in store.Invoices)     _invoices    .Insert(inv,   c, tx);

            tx.Commit();
        }
        catch
        {
            try { tx.Rollback(); } catch { /* swallow rollback errors */ }
            throw;
        }
    }

    private static void Replace<T>(System.ComponentModel.BindingList<T> list, IEnumerable<T> items)
    {
        while (list.Count > 0) list.RemoveAt(list.Count - 1);
        foreach (var item in items) list.Add(item);
    }
}
