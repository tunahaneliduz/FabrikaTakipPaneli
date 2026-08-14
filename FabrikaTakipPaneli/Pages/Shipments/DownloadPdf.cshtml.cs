using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;
using FabrikaTakipPaneli.Services;

namespace FabrikaTakipPaneli.Pages.Shipments;

public class DownloadPdfModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DownloadPdfModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var shipment = await _context.Shipments
            .Include(s => s.StockEntry)
            .ThenInclude(e => e!.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shipment is null)
        {
            return NotFound();
        }

        var quantity = shipment.StockEntry?.Quantity ?? 0;
        var unitPrice = shipment.StockEntry?.UnitPrice;

        var model = new ShipmentPdfModel
        {
            OrderNumber = shipment.OrderNumber,
            ProductName = shipment.StockEntry?.Product?.Name,
            ProductId = shipment.StockEntry?.Product?.Id,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalAmount = quantity * (unitPrice ?? 0),
            CertificateInfo = shipment.CertificateInfo,
            Destination = shipment.Destination,
            TruckPlate = shipment.TruckPlate,
            TruckCapacity = shipment.TruckCapacity,
            IsFullLoad = shipment.IsFullLoad,
            DriverName = shipment.DriverName,
            DriverPhone = shipment.DriverPhone,
            DepartureTime = shipment.DepartureTime,
            StatusLabel = shipment.EffectiveStatus.ToDisplayLabel()
        };

        var pdfBytes = new ShipmentPdfDocument(model).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{shipment.OrderNumber}.pdf");
    }
}
