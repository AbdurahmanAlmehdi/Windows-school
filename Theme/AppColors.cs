namespace HotelManagement.WinForms.Theme;

public static class AppColors
{
    public static readonly Color Primary = ColorTranslator.FromHtml("#1B2A4A");
    public static readonly Color Accent = ColorTranslator.FromHtml("#D4A853");
    public static readonly Color Tertiary = ColorTranslator.FromHtml("#2D5F5D");
    public static readonly Color Surface = ColorTranslator.FromHtml("#FAFAF7");
    public static readonly Color StatusOOS = ColorTranslator.FromHtml("#8B3A3A");
    public static readonly Color StatusClean = ColorTranslator.FromHtml("#C4722A");

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

    public static Color GetRoomStatusColor(Models.RoomStatus status) => status switch
    {
        Models.RoomStatus.Available => Tertiary,
        Models.RoomStatus.Occupied => Primary,
        Models.RoomStatus.NeedsCleaning => StatusClean,
        Models.RoomStatus.OutOfService => StatusOOS,
        _ => Gray500
    };
}
