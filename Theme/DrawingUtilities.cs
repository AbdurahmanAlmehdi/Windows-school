using System.Drawing.Drawing2D;

namespace HotelManagement.WinForms.Theme;

public static class DrawingUtilities
{
    public static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(d, d));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - d;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - d;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void PaintCardBackground(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel panel) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(1, 1, panel.Width - 3, panel.Height - 3);
        using var shadowPath = CreateRoundedRect(new Rectangle(2, 3, panel.Width - 3, panel.Height - 3), 12);
        using var shadowBrush = new SolidBrush(AppColors.ShadowColor);
        g.FillPath(shadowBrush, shadowPath);

        using var path = CreateRoundedRect(rect, 12);
        using var fill = new SolidBrush(AppColors.CardBackground);
        g.FillPath(fill, path);

        panel.Region = null;
    }

    public static void DrawProgressArc(Graphics g, Rectangle rect, float percent, Color fg, Color bg, int thickness)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var inflated = Rectangle.Inflate(rect, -thickness / 2, -thickness / 2);

        using var bgPen = new Pen(bg, thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawArc(bgPen, inflated, 0, 360);

        if (percent > 0)
        {
            var sweep = percent / 100f * 360f;
            using var fgPen = new Pen(fg, thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawArc(fgPen, inflated, -90, sweep);
        }
    }
}
