using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Products;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Product Product { get; set; } = default!;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        Product = product;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (DbUpdateException)
        {
            Product = product;
            ErrorMessage = "Bu ürüne ait stok hareketleri olduğu için silinemiyor.";
            return Page();
        }
    }
}
