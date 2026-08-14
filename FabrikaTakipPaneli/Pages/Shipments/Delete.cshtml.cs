using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;

namespace FabrikaTakipPaneli.Pages.Shipments;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? ProductName { get; set; }

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

        Id = shipment.Id;
        OrderNumber = shipment.OrderNumber;
        Destination = shipment.Destination;
        ProductName = shipment.StockEntry?.Product?.Name;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var shipment = await _context.Shipments.FindAsync(id);
        if (shipment is null)
        {
            return NotFound();
        }

        _context.Shipments.Remove(shipment);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
