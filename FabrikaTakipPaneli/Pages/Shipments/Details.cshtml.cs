using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Authorization;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Shipments;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Shipment Shipment { get; set; } = default!;

    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal TotalAmount => Quantity * (UnitPrice ?? 0);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var shipment = await _context.Shipments
            .Include(s => s.StockEntry)
            .ThenInclude(e => e!.Product)
            .Include(s => s.Driver)
            .Include(s => s.Vehicle)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shipment is null)
        {
            return NotFound();
        }

        Shipment = shipment;
        ProductId = shipment.StockEntry?.Product?.Id;
        ProductName = shipment.StockEntry?.Product?.Name;
        Quantity = shipment.StockEntry?.Quantity ?? 0;
        UnitPrice = shipment.StockEntry?.UnitPrice;

        return Page();
    }

    public async Task<IActionResult> OnPostSetOverrideAsync(int id, ShipmentStatus? overrideStatus)
    {
        if (!User.IsInRole(AppRoles.Admin))
        {
            return Forbid();
        }

        var shipment = await _context.Shipments.FindAsync(id);
        if (shipment is null)
        {
            return NotFound();
        }

        shipment.ManualStatusOverride = overrideStatus;
        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
}
