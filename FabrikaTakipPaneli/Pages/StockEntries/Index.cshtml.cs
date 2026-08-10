using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.StockEntries;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<StockEntryListItem> StockEntries { get; set; } = new List<StockEntryListItem>();

    public async Task OnGetAsync()
    {
        StockEntries = await _context.StockEntries
            .OrderByDescending(s => s.EntryDate)
            .Select(s => new StockEntryListItem
            {
                Id = s.Id,
                ProductName = s.Product!.Name,
                Type = s.Type,
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice,
                EntryDate = s.EntryDate,
                UserName = s.User!.UserName,
                Note = s.Note
            })
            .ToListAsync();
    }

    public class StockEntryListItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public StockEntryType Type { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public DateTime EntryDate { get; set; }
        public string? UserName { get; set; }
        public string? Note { get; set; }
    }
}
