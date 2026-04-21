namespace HotelManagement.WinForms.Models;

public enum RoomStatus
{
    Available,
    Occupied,
    NeedsCleaning,
    OutOfService
}

public enum RoomType
{
    Single,
    Double,
    Suite,
    Deluxe,
    Penthouse
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    CheckedIn,
    Cancelled,
    Completed
}

public enum StayStatus
{
    Active,
    CheckedOut
}

public enum OrderStatus
{
    Placed,
    Preparing,
    Ready,
    Served,
    Cancelled
}

public enum UserRole
{
    Staff,
    Manager
}
