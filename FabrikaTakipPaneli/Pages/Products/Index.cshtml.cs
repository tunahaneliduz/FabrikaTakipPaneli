using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Products;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<ProductListItem> Products { get; set; } = new List<ProductListItem>();

    public async Task OnGetAsync()
    {
        Products = await _context.Products
            .OrderBy(p => p.Name)
            .Select(p => new ProductListItem
            {
                Id = p.Id,
                Name = p.Name,
                Unit = p.Unit,
                Category = p.Category,
                UnitPrice = p.UnitPrice,
                CurrentStock = p.StockEntries.Sum(s => s.Type == StockEntryType.In ? s.Quantity : -s.Quantity)
            })
            .ToListAsync();
    }

    public class ProductListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal CurrentStock { get; set; }
    }
}
