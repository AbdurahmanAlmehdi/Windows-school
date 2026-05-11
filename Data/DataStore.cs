using System.ComponentModel;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Data;

public class DataStore
{
    public BindingList<Room> Rooms { get; } = new();
    public BindingList<Guest> Guests { get; } = new();
    public BindingList<Reservation> Reservations { get; } = new();
    public BindingList<Stay> Stays { get; } = new();
    public BindingList<MenuItem> MenuItems { get; } = new();
    public BindingList<RestaurantOrder> Orders { get; } = new();
    public BindingList<User> Users { get; } = new();
    public BindingList<Invoice> Invoices { get; } = new();
    public BindingList<Role> Roles { get; } = new();

    public DataStore()
    {
        SeedData.Populate(this);
    }
}
