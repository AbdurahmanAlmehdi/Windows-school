using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelManagement.WinForms.Reports;

// Front-of-house operational snapshot - FR-RPT-1, FR-RPT-2, FR-RPT-5.
// Generated on demand from the Reports tab.
public sealed class OccupancyReportPdf : IDocument
{
    private static readonly string PrimaryNavy = Colors.Blue.Darken4;
    private static readonly string AccentGold  = "#D4AF37";

    private readonly DataStore _store;
    private readonly ReportService _reports;
    private readonly DateTime _generatedAt;

    public OccupancyReportPdf(DataStore store, ReportService reports, DateTime generatedAt)
    {
        _store = store;
        _reports = reports;
        _generatedAt = generatedAt;
    }

    public static void Save(DataStore store, ReportService reports, DateTime generatedAt, string filePath)
    {
        Document.Create(new OccupancyReportPdf(store, reports, generatedAt).Compose).GeneratePdf(filePath);
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title   = "Occupancy Report",
        Author  = "Hotel Management System",
        Subject = $"Occupancy snapshot as of {_generatedAt:MMM dd, yyyy HH:mm}",
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.PageColor(Colors.White);
            // Use QuestPDF's bundled Lato (no explicit FontFamily) so the PDF
            // renders identically regardless of host fonts.
            page.DefaultTextStyle(t => t.FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text(
                $"Page X of Y").FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Occupancy Report").FontSize(20).SemiBold().FontColor(PrimaryNavy);
                c.Item().Text("Hotel Management System").FontSize(10).FontColor(Colors.Grey.Darken1);
            });
            row.ConstantItem(180).AlignRight().Column(c =>
            {
                c.Item().AlignRight().Text("Generated").FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().AlignRight().Text(_generatedAt.ToString("MMM dd, yyyy"))
                    .FontSize(12).SemiBold().FontColor(AccentGold);
                c.Item().AlignRight().Text(_generatedAt.ToString("HH:mm"))
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        var rate     = _reports.GetOccupancyRate();
        var total    = _store.Rooms.Count;
        var occupied = _store.Rooms.Count(r => r.IsOccupied);
        var clean    = _store.Rooms.Count(r => r.Condition == RoomCondition.Clean);
        var cleaning = _store.Rooms.Count(r => r.Condition == RoomCondition.NeedsCleaning);
        var oos      = _store.Rooms.Count(r => r.Condition == RoomCondition.OutOfService);

        container.PaddingVertical(20).Column(col =>
        {
            col.Spacing(14);

            // Headline KPI
            col.Item().Background(Colors.Grey.Lighten4).Padding(16).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Occupancy rate").FontSize(11).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"{rate:F1}%").FontSize(28).SemiBold().FontColor(PrimaryNavy);
                });
                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text($"{occupied} / {total} rooms occupied")
                        .FontSize(11).FontColor(Colors.Grey.Darken2);
                    c.Item().AlignRight().Text($"Avg. stay: {_reports.GetAverageStayDuration():F1} days")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            });

            // Status breakdown
            col.Item().Text("Status breakdown").FontSize(13).SemiBold().FontColor(PrimaryNavy);
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                void Stat(string label, string value)
                {
                    t.Cell().Element(StatLabelCell).Text(label).FontColor(Colors.Grey.Darken2);
                    t.Cell().Element(StatValueCell).AlignRight().Text(value).SemiBold().FontColor(PrimaryNavy);
                }

                Stat("Clean & available", clean.ToString());
                Stat("Occupied",          occupied.ToString());
                Stat("Needs cleaning",    cleaning.ToString());
                Stat("Out of service",    oos.ToString());
            });

            // Per-type breakdown
            col.Item().Text("Rooms by type").FontSize(13).SemiBold().FontColor(PrimaryNavy);
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                t.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Type");
                    h.Cell().Element(HeaderCell).AlignCenter().Text("Total");
                    h.Cell().Element(HeaderCell).AlignCenter().Text("Occupied");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Avg rate");
                });

                foreach (var type in Enum.GetValues<RoomType>())
                {
                    var roomsOfType = _store.Rooms.Where(r => r.Type == type).ToList();
                    if (roomsOfType.Count == 0) continue;

                    var occCount = roomsOfType.Count(r => r.IsOccupied);
                    var avgRate  = roomsOfType.Average(r => r.Rate);

                    t.Cell().Element(BodyCell).Text(type.ToString());
                    t.Cell().Element(BodyCell).AlignCenter().Text(roomsOfType.Count.ToString());
                    t.Cell().Element(BodyCell).AlignCenter().Text(occCount.ToString());
                    t.Cell().Element(BodyCell).AlignRight().Text($"${avgRate:F2}");
                }
            });

            // Currently in-house guests
            var active = _store.Stays.Where(s => s.Status == StayStatus.Active).ToList();
            if (active.Count > 0)
            {
                col.Item().Text($"In-house guests ({active.Count})")
                    .FontSize(13).SemiBold().FontColor(PrimaryNavy);

                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);   // room
                        c.RelativeColumn(4);   // guest
                        c.RelativeColumn(3);   // check-in
                        c.RelativeColumn(3);   // expected out
                    });
                    t.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Room");
                        h.Cell().Element(HeaderCell).Text("Guest");
                        h.Cell().Element(HeaderCell).Text("Check-in");
                        h.Cell().Element(HeaderCell).Text("Expected out");
                    });
                    foreach (var stay in active.OrderBy(s => s.Room.Number))
                    {
                        t.Cell().Element(BodyCell).Text(stay.Room.Number.ToString());
                        t.Cell().Element(BodyCell).Text(stay.Guest.Name);
                        t.Cell().Element(BodyCell).Text(stay.CheckInDate.ToString("MMM dd"));
                        t.Cell().Element(BodyCell).Text(stay.ExpectedCheckOut.ToString("MMM dd"));
                    }
                });
            }
        });
    }

    private static IContainer HeaderCell(IContainer c) =>
        c.DefaultTextStyle(t => t.SemiBold().FontColor(Colors.White))
         .Background(PrimaryNavy)
         .Padding(6);

    private static IContainer BodyCell(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6);

    private static IContainer StatLabelCell(IContainer c) =>
        c.Padding(8).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

    private static IContainer StatValueCell(IContainer c) =>
        c.Padding(8).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
}
