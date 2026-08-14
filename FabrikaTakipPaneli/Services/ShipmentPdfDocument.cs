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
    public string StatusLabel { get; set; } = string.Empty;
}

public class ShipmentPdfDocument : IDocument
{
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
            page.Margin(40);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(x => x.FontSize(11));

            page.Header().Text($"Sevkiyat Raporu - {_model.OrderNumber}").FontSize(18).Bold();

            page.Content().PaddingVertical(15).Column(column =>
            {
                column.Spacing(8);

                column.Item().Text("Ürün Bilgisi").Bold().FontSize(13);
                column.Item().Text($"Ürün: {_model.ProductName} (#{_model.ProductId})");
                column.Item().Text($"Miktar: {_model.Quantity:N2}");
                column.Item().Text($"Birim Fiyat: {(_model.UnitPrice.HasValue ? _model.UnitPrice.Value.ToString("N2") : "-")}");
                column.Item().Text($"Toplam Tutar: {_model.TotalAmount:N2}");
                column.Item().Text($"Sertifika Bilgisi: {_model.CertificateInfo ?? "-"}");

                column.Item().PaddingTop(10).Text("Sevkiyat Detayları").Bold().FontSize(13);
                column.Item().Text($"Varış Yeri: {_model.Destination}");
                column.Item().Text($"Araç Plakası: {_model.TruckPlate ?? "-"}");
                column.Item().Text($"Araç Kapasitesi: {_model.TruckCapacity ?? "-"} ({(_model.IsFullLoad ? "Dolu" : "Boş")})");
                column.Item().Text($"Sürücü: {_model.DriverName ?? "-"} {_model.DriverPhone ?? string.Empty}");
                column.Item().Text($"Yola Çıkış: {_model.DepartureTime:g}");
                column.Item().Text($"Durum: {_model.StatusLabel}");
            });

            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Oluşturulma: ").FontSize(9);
                text.Span(DateTime.Now.ToString("g")).FontSize(9);
            });
        });
    }
}
