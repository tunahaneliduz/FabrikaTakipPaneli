using Microsoft.AspNetCore.Mvc;
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

    private const int PageSize = 10;

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public int TotalPages { get; set; }

    public IList<ProductListItem> Products { get; set; } = new List<ProductListItem>();

    public async Task OnGetAsync()
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{SearchTerm}%"));
        }

        var totalCount = await query.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        PageIndex = Math.Clamp(PageIndex, 1, TotalPages);

        Products = await query
            .OrderBy(p => p.Name)
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
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
