using HotelManagement.WinForms.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelManagement.WinForms.Reports;

// FR-RPT-3 (restaurant revenue by category) + FR-RPT-4 (top menu items)
// + FR-RPT-7 (top-line revenue), rolled into one operational PDF.
public sealed class RestaurantRevenuePdf : IDocument
{
    private static readonly string PrimaryNavy = Colors.Blue.Darken4;
    private static readonly string AccentGold  = "#D4AF37";

    private readonly ReportService _reports;
    private readonly InvoiceService _invoices;
    private readonly DateTime _generatedAt;

    public RestaurantRevenuePdf(ReportService reports, InvoiceService invoices, DateTime generatedAt)
    {
        _reports = reports;
        _invoices = invoices;
        _generatedAt = generatedAt;
    }

    public static void Save(ReportService reports, InvoiceService invoices, DateTime generatedAt, string filePath)
    {
        Document.Create(new RestaurantRevenuePdf(reports, invoices, generatedAt).Compose).GeneratePdf(filePath);
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title   = "Restaurant Revenue Report",
        Author  = "Hotel Management System",
        Subject = $"Revenue snapshot as of {_generatedAt:MMM dd, yyyy HH:mm}",
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontFamily(Fonts.SegoeUI).FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text("Hotel Management System")
                .FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Restaurant Revenue").FontSize(20).SemiBold().FontColor(PrimaryNavy);
                c.Item().Text("Hotel Management System").FontSize(10).FontColor(Colors.Grey.Darken1);
            });
            row.ConstantItem(180).AlignRight().Column(c =>
            {
                c.Item().AlignRight().Text("Generated").FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().AlignRight().Text(_generatedAt.ToString("MMM dd, yyyy"))
                    .FontSize(12).SemiBold().FontColor(AccentGold);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        var totalRevenue = _invoices.GetTotalRevenue();
        var outstanding  = _invoices.GetOutstandingAmount();
        var todayRev     = _invoices.GetTodayRevenue();

        var byCategory = _reports.GetRestaurantRevenueByCategory();
        var topItems   = _reports.GetTopMenuItems(10);

        container.PaddingVertical(20).Column(col =>
        {
            col.Spacing(14);

            // Headline KPIs
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(c => Kpi(c, "Total revenue (paid)", $"${totalRevenue:F2}"));
                row.ConstantItem(12);
                row.RelativeItem().Element(c => Kpi(c, "Outstanding (pending)", $"${outstanding:F2}"));
                row.ConstantItem(12);
                row.RelativeItem().Element(c => Kpi(c, "Today's revenue",      $"${todayRev:F2}"));
            });

            // Restaurant revenue by category
            col.Item().Text("Revenue by category").FontSize(13).SemiBold().FontColor(PrimaryNavy);
            if (byCategory.Count == 0)
            {
                col.Item().Text("No restaurant orders recorded.")
                    .FontSize(10).FontColor(Colors.Grey.Darken1).Italic();
            }
            else
            {
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                    t.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Category");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Revenue");
                    });
                    foreach (var (cat, amount) in byCategory.OrderByDescending(kv => kv.Value))
                    {
                        t.Cell().Element(BodyCell).Text(cat);
                        t.Cell().Element(BodyCell).AlignRight().Text($"${amount:F2}");
                    }
                });
            }

            // Top menu items
            col.Item().Text("Top menu items").FontSize(13).SemiBold().FontColor(PrimaryNavy);
            if (topItems.Count == 0)
            {
                col.Item().Text("No menu orders recorded.")
                    .FontSize(10).FontColor(Colors.Grey.Darken1).Italic();
            }
            else
            {
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(32);
                        c.RelativeColumn(5);
                        c.RelativeColumn(2);
                    });
                    t.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).AlignCenter().Text("#");
                        h.Cell().Element(HeaderCell).Text("Item");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Units sold");
                    });
                    int rank = 1;
                    foreach (var item in topItems)
                    {
                        t.Cell().Element(BodyCell).AlignCenter().Text(rank.ToString());
                        t.Cell().Element(BodyCell).Text(item.Name);
                        t.Cell().Element(BodyCell).AlignRight().Text(item.Count.ToString());
                        rank++;
                    }
                });
            }
        });
    }

    private static void Kpi(IContainer container, string label, string value)
    {
        container.Background(Colors.Grey.Lighten4).Padding(12).Column(c =>
        {
            c.Item().Text(label).FontSize(10).FontColor(Colors.Grey.Darken2);
            c.Item().Text(value).FontSize(18).SemiBold().FontColor(PrimaryNavy);
        });
    }

    private static IContainer HeaderCell(IContainer c) =>
        c.DefaultTextStyle(t => t.SemiBold().FontColor(Colors.White))
         .Background(PrimaryNavy)
         .Padding(6);

    private static IContainer BodyCell(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6);
}
