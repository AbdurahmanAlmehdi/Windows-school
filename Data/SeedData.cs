using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Data;

public static class SeedData
{
    public static void Populate(DataStore store)
    {
        // Users
        store.Users.Add(new User { Username = "admin", Password = "admin123", Role = UserRole.Manager });
        store.Users.Add(new User { Username = "staff", Password = "staff123", Role = UserRole.Staff });

        // Rooms
        store.Rooms.Add(new Room { Number = 101, Type = RoomType.Single, Rate = 99.99m, Status = RoomStatus.Available });
        store.Rooms.Add(new Room { Number = 102, Type = RoomType.Single, Rate = 99.99m, Status = RoomStatus.Available });
        store.Rooms.Add(new Room { Number = 201, Type = RoomType.Double, Rate = 149.99m, Status = RoomStatus.Occupied });
        store.Rooms.Add(new Room { Number = 202, Type = RoomType.Double, Rate = 149.99m, Status = RoomStatus.Available });
        store.Rooms.Add(new Room { Number = 301, Type = RoomType.Suite, Rate = 249.99m, Status = RoomStatus.Occupied });
        store.Rooms.Add(new Room { Number = 302, Type = RoomType.Suite, Rate = 249.99m, Status = RoomStatus.NeedsCleaning });
        store.Rooms.Add(new Room { Number = 401, Type = RoomType.Deluxe, Rate = 349.99m, Status = RoomStatus.Available });
        store.Rooms.Add(new Room { Number = 402, Type = RoomType.Deluxe, Rate = 349.99m, Status = RoomStatus.Available });
        store.Rooms.Add(new Room { Number = 501, Type = RoomType.Penthouse, Rate = 599.99m, Status = RoomStatus.OutOfService, MaintenanceLog = "Plumbing repair scheduled" });
        store.Rooms.Add(new Room { Number = 502, Type = RoomType.Penthouse, Rate = 599.99m, Status = RoomStatus.Available });

        // Guests
        var guest1 = new Guest { Name = "John Smith", Contact = "555-0101", IsVip = true, StayCount = 5 };
        var guest2 = new Guest { Name = "Jane Doe", Contact = "555-0102", IsVip = false, StayCount = 2 };
        var guest3 = new Guest { Name = "Bob Johnson", Contact = "555-0103", IsVip = false, StayCount = 1 };
        var guest4 = new Guest { Name = "Alice Williams", Contact = "555-0104", IsVip = true, StayCount = 8 };
        var guest5 = new Guest { Name = "Charlie Brown", Contact = "555-0105", IsVip = false, StayCount = 0 };
        store.Guests.Add(guest1);
        store.Guests.Add(guest2);
        store.Guests.Add(guest3);
        store.Guests.Add(guest4);
        store.Guests.Add(guest5);

        // Reservations
        store.Reservations.Add(new Reservation
        {
            Guest = guest5,
            Room = store.Rooms[3], // 202
            CheckInDate = DateTime.Today.AddDays(2),
            CheckOutDate = DateTime.Today.AddDays(5),
            Status = ReservationStatus.Confirmed
        });
        store.Reservations.Add(new Reservation
        {
            Guest = guest3,
            Room = store.Rooms[7], // 402
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            Status = ReservationStatus.Confirmed
        });
        store.Reservations.Add(new Reservation
        {
            Guest = guest2,
            Room = store.Rooms[0], // 101
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(2),
            Status = ReservationStatus.Confirmed
        });

        // Active Stays
        var stay1 = new Stay
        {
            Guest = guest1,
            Room = store.Rooms[2], // 201
            CheckInDate = DateTime.Today.AddDays(-2),
            ExpectedCheckOut = DateTime.Today.AddDays(1),
            RoomCharges = 299.98m,
            Status = StayStatus.Active
        };
        var stay2 = new Stay
        {
            Guest = guest4,
            Room = store.Rooms[4], // 301
            CheckInDate = DateTime.Today.AddDays(-1),
            ExpectedCheckOut = DateTime.Today.AddDays(3),
            RoomCharges = 249.99m,
            Status = StayStatus.Active
        };
        store.Stays.Add(stay1);
        store.Stays.Add(stay2);

        // Menu Items
        store.MenuItems.Add(new MenuItem { Name = "Caesar Salad", Price = 12.99m, Category = "Starters" });
        store.MenuItems.Add(new MenuItem { Name = "Tomato Soup", Price = 8.99m, Category = "Starters" });
        store.MenuItems.Add(new MenuItem { Name = "Grilled Salmon", Price = 24.99m, Category = "Main Course" });
        store.MenuItems.Add(new MenuItem { Name = "Beef Steak", Price = 32.99m, Category = "Main Course" });
        store.MenuItems.Add(new MenuItem { Name = "Chicken Pasta", Price = 18.99m, Category = "Main Course" });
        store.MenuItems.Add(new MenuItem { Name = "Margherita Pizza", Price = 15.99m, Category = "Main Course" });
        store.MenuItems.Add(new MenuItem { Name = "Chocolate Cake", Price = 9.99m, Category = "Desserts" });
        store.MenuItems.Add(new MenuItem { Name = "Ice Cream Sundae", Price = 7.99m, Category = "Desserts" });
        store.MenuItems.Add(new MenuItem { Name = "Tiramisu", Price = 10.99m, Category = "Desserts" });
        store.MenuItems.Add(new MenuItem { Name = "Fresh Orange Juice", Price = 5.99m, Category = "Beverages" });
        store.MenuItems.Add(new MenuItem { Name = "Espresso", Price = 3.99m, Category = "Beverages" });
        store.MenuItems.Add(new MenuItem { Name = "Cappuccino", Price = 4.99m, Category = "Beverages" });
        store.MenuItems.Add(new MenuItem { Name = "Mineral Water", Price = 2.99m, Category = "Beverages" });
        store.MenuItems.Add(new MenuItem { Name = "Club Sandwich", Price = 14.99m, Category = "Snacks" });
        store.MenuItems.Add(new MenuItem { Name = "French Fries", Price = 6.99m, Category = "Snacks" });

        // Sample order for stay1
        var order = new RestaurantOrder { Stay = stay1, Status = OrderStatus.Served };
        order.Lines.Add(new OrderLine { MenuItem = store.MenuItems[2], Quantity = 1 }); // Grilled Salmon
        order.Lines.Add(new OrderLine { MenuItem = store.MenuItems[10], Quantity = 2 }); // Espresso
        store.Orders.Add(order);
        stay1.RestaurantCharges = order.Total;

        // --- Sample Invoices ---

        // Past guest with completed stay and paid invoice
        var guest6 = new Guest { Name = "Diana Prince", Contact = "555-0106", IsVip = false, StayCount = 1 };
        store.Guests.Add(guest6);

        var pastStay = new Stay
        {
            Guest = guest6,
            Room = store.Rooms[5], // 302
            CheckInDate = DateTime.Today.AddDays(-7),
            ExpectedCheckOut = DateTime.Today.AddDays(-4),
            ActualCheckOut = DateTime.Today.AddDays(-4),
            RoomCharges = 749.97m,
            Status = StayStatus.CheckedOut
        };
        store.Stays.Add(pastStay);

        var pastRes = new Reservation
        {
            Guest = guest6,
            Room = store.Rooms[5],
            CheckInDate = DateTime.Today.AddDays(-7),
            CheckOutDate = DateTime.Today.AddDays(-4),
            Status = ReservationStatus.Completed
        };
        store.Reservations.Add(pastRes);

        var invoice1 = new Invoice("INV-1001")
        {
            Stay = pastStay,
            Guest = guest6,
            Room = store.Rooms[5],
            InvoiceDate = DateTime.Today.AddDays(-4),
            PaymentStatus = PaymentStatus.Paid,
            PaymentMethod = Models.PaymentMethod.CreditCard,
            PaymentDate = DateTime.Today.AddDays(-4)
        };
        for (int i = 0; i < 3; i++)
        {
            invoice1.Lines.Add(new InvoiceLine
            {
                Description = $"Room 302 - Night {i + 1} ({DateTime.Today.AddDays(-7 + i):MMM dd})",
                Quantity = 1,
                UnitPrice = 249.99m,
                Category = InvoiceLineCategory.RoomCharge
            });
        }
        invoice1.Lines.Add(new InvoiceLine
        {
            Description = "Beef Steak",
            Quantity = 1,
            UnitPrice = 32.99m,
            Category = InvoiceLineCategory.RestaurantCharge
        });
        invoice1.Lines.Add(new InvoiceLine
        {
            Description = "Cappuccino",
            Quantity = 2,
            UnitPrice = 4.99m,
            Category = InvoiceLineCategory.RestaurantCharge
        });
        store.Invoices.Add(invoice1);

        // Pending invoice for demonstration (Bob Johnson's upcoming checkout)
        var invoice2 = new Invoice("INV-1002")
        {
            Stay = stay2,
            Guest = guest4,
            Room = store.Rooms[4],
            InvoiceDate = DateTime.Today,
            PaymentStatus = PaymentStatus.Pending
        };
        invoice2.Lines.Add(new InvoiceLine
        {
            Description = $"Room 301 - Night 1 ({DateTime.Today.AddDays(-1):MMM dd})",
            Quantity = 1,
            UnitPrice = 249.99m,
            Category = InvoiceLineCategory.RoomCharge
        });
        store.Invoices.Add(invoice2);
    }
}
