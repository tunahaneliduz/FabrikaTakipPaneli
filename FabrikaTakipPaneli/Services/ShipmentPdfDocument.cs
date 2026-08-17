using System.Globalization;
using FabrikaTakipPaneli.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FabrikaTakipPaneli.Services;

public class ShipmentPdfModel
{
    public string OrderNumber { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public int? ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CertificateInfo { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string? TruckPlate { get; set; }
    public string? TruckCapacity { get; set; }
    public bool IsFullLoad { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public DateTime DepartureTime { get; set; }
    public ShipmentStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
}

public class ShipmentPdfDocument : IDocument
{
    private static readonly string BorderColor = Colors.Grey.Darken1;
    private static readonly string HeaderBackground = Colors.Grey.Lighten3;

    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    private readonly ShipmentPdfModel _model;

    public ShipmentPdfDocument(ShipmentPdfModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(36);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

            page.Header().Element(ComposeHeader);
            page.Content().PaddingTop(16).Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(inner =>
                {
                    inner.Item().Text("FABRİKATAKİPPANELİ").Bold().FontSize(11).LetterSpacing(0.05f).FontColor(Colors.Grey.Darken2);
                    inner.Item().Text("SEVKİYAT RAPORU").Bold().FontSize(20);
                });

                row.ConstantItem(180).Column(inner =>
                {
                    inner.Item().AlignRight().Text("Seri No").FontSize(9).FontColor(Colors.Grey.Darken2);
                    inner.Item().AlignRight().Text(_model.OrderNumber).Bold().FontSize(16);
                    inner.Item().PaddingTop(4).AlignRight().Element(c => ComposeStatusBadge(c));
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1.5f).LineColor(Colors.Black);
        });
    }

    private void ComposeStatusBadge(IContainer container)
    {
        var (background, textColor) = _model.Status switch
        {
            ShipmentStatus.Yolda => (Colors.Orange.Lighten4, Colors.Orange.Darken3),
            ShipmentStatus.TeslimEdiliyor => (Colors.Blue.Lighten4, Colors.Blue.Darken3),
            ShipmentStatus.TeslimEdildi => (Colors.Green.Lighten4, Colors.Green.Darken3),
            _ => (Colors.Grey.Lighten3, Colors.Grey.Darken3)
        };

        container.Background(background)
            .Border(1)
            .BorderColor(textColor)
            .PaddingVertical(3)
            .PaddingHorizontal(8)
            .Text(_model.StatusLabel.ToUpper(TurkishCulture))
            .Bold()
            .FontSize(9)
            .FontColor(textColor);
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(16);

            column.Item().Element(c => ComposeSection(c, "ÜRÜN BİLGİSİ", section =>
            {
                section.Row("Ürün", $"{_model.ProductName} (#{_model.ProductId})");
                section.Row("Miktar", _model.Quantity.ToString("N2"));
                section.Row("Birim Fiyat", _model.UnitPrice.HasValue ? _model.UnitPrice.Value.ToString("N2") : "-");
                section.Row("Toplam Tutar", _model.TotalAmount.ToString("N2"));
                section.Row("Sertifika Bilgisi", _model.CertificateInfo ?? "-");
            }));

            column.Item().Element(c => ComposeSection(c, "SEVKİYAT DETAYLARI", section =>
            {
                section.Row("Varış Yeri", _model.Destination);
                section.Row("Araç Plakası", _model.TruckPlate ?? "-");
                section.Row("Araç Kapasitesi", $"{_model.TruckCapacity ?? "-"} ({(_model.IsFullLoad ? "Dolu" : "Boş")})");
                section.Row("Sürücü", _model.DriverName ?? "-");
                section.Row("Sürücü Telefonu", _model.DriverPhone ?? "-");
                section.Row("Yola Çıkış", _model.DepartureTime.ToString("g"));
            }));
        });
    }

    private void ComposeSection(IContainer container, string title, Action<SectionBuilder> build)
    {
        container.Column(column =>
        {
            column.Item()
                .Background(HeaderBackground)
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(6)
                .Text(title).Bold().FontSize(11).LetterSpacing(0.03f);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                var builder = new SectionBuilder(table);
                build(builder);
            });
        });
    }

    private sealed class SectionBuilder
    {
        private readonly TableDescriptor _table;
        private bool _alternate;

        public SectionBuilder(TableDescriptor table)
        {
            _table = table;
        }

        public void Row(string label, string value)
        {
            var background = _alternate ? Colors.Grey.Lighten5 : Colors.White;
            _alternate = !_alternate;

            _table.Cell().Background(background).Border(1).BorderColor(BorderColor).Padding(6)
                .Text(label).SemiBold().FontColor(Colors.Grey.Darken3);

            _table.Cell().Background(background).Border(1).BorderColor(BorderColor).Padding(6)
                .Text(value);
        }
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(4).LineHorizontal(0.75f).LineColor(BorderColor);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("Bu belge otomatik oluşturulmuştur.").FontSize(8).FontColor(Colors.Grey.Darken1).Italic();
                row.RelativeItem().AlignRight().Text($"Oluşturulma: {DateTime.Now:g}").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }
}
