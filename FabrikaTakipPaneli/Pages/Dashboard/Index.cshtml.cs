using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Dashboard;

public class IndexModel : PageModel
{
    private const int LowStockListCount = 5;
    private const int LowStockChartCount = 10;
    private const int RecentEntriesCount = 10;
    private const int TrendDays = 14;

    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalProductCount { get; set; }
    public int TotalStockEntryCount { get; set; }
    public int TodayEntryCount { get; set; }
    public int CriticalStockProductCount { get; set; }

    public IList<LowStockItem> LowStockProducts { get; set; } = new List<LowStockItem>();
    public IList<RecentEntryItem> RecentEntries { get; set; } = new List<RecentEntryItem>();

    public IList<string> LowStockChartLabels { get; set; } = new List<string>();
    public IList<decimal> LowStockChartValues { get; set; } = new List<decimal>();

    public IList<string> TrendChartLabels { get; set; } = new List<string>();
    public IList<decimal> TrendChartInValues { get; set; } = new List<decimal>();
    public IList<decimal> TrendChartOutValues { get; set; } = new List<decimal>();

    public async Task OnGetAsync()
    {
        var today = DateTime.Today;

        TotalProductCount = await _context.Products.CountAsync();
        TotalStockEntryCount = await _context.StockEntries.CountAsync();
        TodayEntryCount = await _context.StockEntries.CountAsync(s => s.EntryDate >= today && s.EntryDate < today.AddDays(1));

        var productStocks = await _context.Products
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Unit,
                p.MinStockLevel,
                CurrentStock = p.StockEntries.Sum(s => s.Type == StockEntryType.In ? s.Quantity : -s.Quantity)
            })
            .OrderBy(p => p.CurrentStock)
            .ToListAsync();

        CriticalStockProductCount = productStocks.Count(p => p.CurrentStock <= (p.MinStockLevel ?? 0));

        LowStockProducts = productStocks
            .Take(LowStockListCount)
            .Select(p => new LowStockItem
            {
                Name = p.Name,
                Unit = p.Unit,
                CurrentStock = p.CurrentStock,
                IsCritical = p.CurrentStock <= (p.MinStockLevel ?? 0)
            })
            .ToList();

        var lowStockForChart = productStocks.Take(LowStockChartCount).ToList();
        LowStockChartLabels = lowStockForChart.Select(p => p.Name).ToList();
        LowStockChartValues = lowStockForChart.Select(p => p.CurrentStock).ToList();

        var recentEntries = await _context.StockEntries
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
        RecentEntries = recentEntries;

        var trendStart = today.AddDays(-(TrendDays - 1));
        var trendEntries = await _context.StockEntries
            .Where(s => s.EntryDate >= trendStart && s.EntryDate < today.AddDays(1))
            .Select(s => new { s.EntryDate, s.Type, s.Quantity })
            .ToListAsync();

        var trendByDate = trendEntries
            .GroupBy(s => s.EntryDate.Date)
            .ToDictionary(g => g.Key, g => new
            {
                In = g.Where(x => x.Type == StockEntryType.In).Sum(x => x.Quantity),
                Out = g.Where(x => x.Type == StockEntryType.Out).Sum(x => x.Quantity)
            });

        for (var day = trendStart; day <= today; day = day.AddDays(1))
        {
            TrendChartLabels.Add(day.ToString("dd.MM"));
            trendByDate.TryGetValue(day, out var totals);
            TrendChartInValues.Add(totals?.In ?? 0);
            TrendChartOutValues.Add(totals?.Out ?? 0);
        }
    }

    public class LowStockItem
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public bool IsCritical { get; set; }
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
