namespace HotelManagement.WinForms.Theme;

public static class AppColors
{
    public static readonly Color Primary = ColorTranslator.FromHtml("#1B2A4A");
    public static readonly Color Accent = ColorTranslator.FromHtml("#D4A853");
    public static readonly Color Tertiary = ColorTranslator.FromHtml("#2D5F5D");
    public static readonly Color Surface = ColorTranslator.FromHtml("#FAFAF7");
    public static readonly Color StatusOOS = ColorTranslator.FromHtml("#8B3A3A");
    public static readonly Color StatusClean = ColorTranslator.FromHtml("#C4722A");

    public static readonly Color ShadowColor = Color.FromArgb(15, 0, 0, 0);
    public static readonly Color CardBackground = Color.White;

    public static readonly Color Gray100 = ColorTranslator.FromHtml("#F5F5F5");
    public static readonly Color Gray200 = ColorTranslator.FromHtml("#E5E5E5");
    public static readonly Color Gray300 = ColorTranslator.FromHtml("#D4D4D4");
    public static readonly Color Gray400 = ColorTranslator.FromHtml("#A3A3A3");
    public static readonly Color Gray500 = ColorTranslator.FromHtml("#737373");
    public static readonly Color Gray600 = ColorTranslator.FromHtml("#525252");
    public static readonly Color Gray700 = ColorTranslator.FromHtml("#404040");
    public static readonly Color Gray800 = ColorTranslator.FromHtml("#262626");
    public static readonly Color Gray900 = ColorTranslator.FromHtml("#171717");
    public static readonly Color Gray950 = ColorTranslator.FromHtml("#0A0A0A");

    public static Color GetRoomCardColor(Models.Room room)
    {
        if (room.Condition == Models.RoomCondition.OutOfService) return StatusOOS;
        if (room.IsOccupied && room.Condition == Models.RoomCondition.NeedsCleaning) return StatusClean;
        if (room.IsOccupied) return Primary;
        if (room.Condition == Models.RoomCondition.NeedsCleaning) return StatusClean;
        return Tertiary; // Available
    }

    public static Color GetRoomStatusBadgeColor(string displayStatus) => displayStatus switch
    {
        "Available" => Tertiary,
        "Occupied" => Primary,
        "Occupied + Needs Cleaning" => StatusClean,
        "Needs Cleaning" => StatusClean,
        "Out of Service" => StatusOOS,
        _ => Gray500
    };

    public static Color GetOrderStatusColor(Models.OrderStatus status) => status switch
    {
        Models.OrderStatus.Placed => Accent,
        Models.OrderStatus.Preparing => StatusClean,
        Models.OrderStatus.Ready => Tertiary,
        Models.OrderStatus.Served => Gray400,
        Models.OrderStatus.Cancelled => StatusOOS,
        _ => Gray500
    };

    public static Color GetPaymentStatusColor(Models.PaymentStatus status) => status switch
    {
        Models.PaymentStatus.Paid => Tertiary,
        Models.PaymentStatus.Pending => Accent,
        Models.PaymentStatus.Refunded => StatusOOS,
        _ => Gray500
    };

    public static Color GetReservationStatusColor(Models.ReservationStatus status) => status switch
    {
        Models.ReservationStatus.Confirmed => Tertiary,
        Models.ReservationStatus.CheckedIn => Primary,
        Models.ReservationStatus.Completed => Gray400,
        Models.ReservationStatus.Cancelled => StatusOOS,
        Models.ReservationStatus.Pending => Accent,
        _ => Gray500
    };
}
