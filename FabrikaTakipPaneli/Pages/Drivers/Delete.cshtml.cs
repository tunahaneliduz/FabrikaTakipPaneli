using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Drivers;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Driver Driver { get; set; } = default!;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver is null)
        {
            return NotFound();
        }

        Driver = driver;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver is null)
        {
            return NotFound();
        }

        _context.Drivers.Remove(driver);

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (DbUpdateException)
        {
            Driver = driver;
            ErrorMessage = "Bu sürücüye ait sevkiyat kayıtları olduğu için silinemiyor.";
            return Page();
        }
    }
}
