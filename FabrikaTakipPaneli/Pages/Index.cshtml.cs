using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages;

public class IndexModel : PageModel
{
    private const int RecentEntriesCount = 5;

    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalProductCount { get; set; }
    public int CriticalStockProductCount { get; set; }
    public int TodayEntryCount { get; set; }

    public IList<RecentEntryItem> RecentEntries { get; set; } = new List<RecentEntryItem>();

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var today = DateTime.Today;

        TotalProductCount = await _context.Products.CountAsync();
        TodayEntryCount = await _context.StockEntries.CountAsync(s => s.EntryDate >= today && s.EntryDate < today.AddDays(1));

        CriticalStockProductCount = await _context.Products
            .Select(p => new
            {
                p.MinStockLevel,
                CurrentStock = p.StockEntries.Sum(s => s.Type == StockEntryType.In ? s.Quantity : -s.Quantity)
            })
            .CountAsync(p => p.CurrentStock <= (p.MinStockLevel ?? 0));

        RecentEntries = await _context.StockEntries
            .OrderByDescending(s => s.EntryDate)
            .Take(RecentEntriesCount)
            .Select(s => new RecentEntryItem
            {
                ProductName = s.Product!.Name,
                Type = s.Type,
                Quantity = s.Quantity,
                EntryDate = s.EntryDate,
                UserName = s.User!.UserName
            })
            .ToListAsync();
    }

    public class RecentEntryItem
    {
        public string ProductName { get; set; } = string.Empty;
        public StockEntryType Type { get; set; }
        public decimal Quantity { get; set; }
        public DateTime EntryDate { get; set; }
        public string? UserName { get; set; }
    }
}
