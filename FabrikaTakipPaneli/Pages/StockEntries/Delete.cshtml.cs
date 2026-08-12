using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Authorization;
using FabrikaTakipPaneli.Data;

namespace FabrikaTakipPaneli.Pages.StockEntries;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DeleteModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public FabrikaTakipPaneli.Models.StockEntryType Type { get; set; }
    public decimal Quantity { get; set; }
    public DateTime EntryDate { get; set; }
    public string? Note { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var stockEntry = await _context.StockEntries
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (stockEntry is null)
        {
            return NotFound();
        }

        if (!StockEntryAccess.CanModify(stockEntry, _userManager.GetUserId(User), User.IsInRole(AppRoles.Admin)))
        {
            return Forbid();
        }

        Id = stockEntry.Id;
        ProductName = stockEntry.Product!.Name;
        Type = stockEntry.Type;
        Quantity = stockEntry.Quantity;
        EntryDate = stockEntry.EntryDate;
        Note = stockEntry.Note;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var stockEntry = await _context.StockEntries.FindAsync(id);
        if (stockEntry is null)
        {
            return NotFound();
        }

        if (!StockEntryAccess.CanModify(stockEntry, _userManager.GetUserId(User), User.IsInRole(AppRoles.Admin)))
        {
            return Forbid();
        }

        _context.StockEntries.Remove(stockEntry);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
