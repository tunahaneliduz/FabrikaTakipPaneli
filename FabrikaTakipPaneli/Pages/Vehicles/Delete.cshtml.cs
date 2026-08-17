using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Vehicles;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Vehicle Vehicle { get; set; } = default!;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        Vehicle = vehicle;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        _context.Vehicles.Remove(vehicle);

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (DbUpdateException)
        {
            Vehicle = vehicle;
            ErrorMessage = "Bu araca ait sevkiyat kayıtları olduğu için silinemiyor.";
            return Page();
        }
    }
}
