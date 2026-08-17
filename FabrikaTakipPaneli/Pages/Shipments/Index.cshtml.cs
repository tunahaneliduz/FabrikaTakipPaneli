using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;
using System.ComponentModel.DataAnnotations;

namespace FabrikaTakipPaneli.Pages.Shipments;

public class IndexModel : PageModel
{
    private const int PageSize = 10;

    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public ShipmentStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public int TotalPages { get; set; }

    public IList<ShipmentListItem> Shipments { get; set; } = new List<ShipmentListItem>();

    public async Task OnGetAsync()
    {
        var query = _context.Shipments
            .Include(s => s.StockEntry)
            .ThenInclude(e => e!.Product)
            .Include(s => s.Driver)
            .Include(s => s.Vehicle)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(s =>
                EF.Functions.Like(s.OrderNumber, $"%{SearchTerm}%") ||
                EF.Functions.Like(s.Destination, $"%{SearchTerm}%"));
        }

        if (StartDate.HasValue)
        {
            query = query.Where(s => s.DepartureTime >= StartDate.Value.Date);
        }

        if (EndDate.HasValue)
        {
            var exclusiveEnd = EndDate.Value.Date.AddDays(1);
            query = query.Where(s => s.DepartureTime < exclusiveEnd);
        }

        var matches = await query
            .OrderByDescending(s => s.DepartureTime)
            .ToListAsync();

        var items = matches.Select(s => new ShipmentListItem
        {
            Id = s.Id,
            OrderNumber = s.OrderNumber,
            Destination = s.Destination,
            DriverName = s.EffectiveDriverName,
            TruckPlate = s.EffectiveTruckPlate,
            DepartureTime = s.DepartureTime,
            Status = s.EffectiveStatus,
            ProductName = s.StockEntry?.Product?.Name,
            Quantity = s.StockEntry?.Quantity,
            UnitPrice = s.StockEntry?.UnitPrice,
        }).AsEnumerable();

        if (StatusFilter.HasValue)
        {
            items = items.Where(i => i.Status == StatusFilter.Value);
        }

        var filtered = items.ToList();

        TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
        PageIndex = Math.Clamp(PageIndex, 1, TotalPages);

        Shipments = filtered
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    public class ShipmentListItem
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string? DriverName { get; set; }
        public string? TruckPlate { get; set; }
        public DateTime DepartureTime { get; set; }
        public ShipmentStatus Status { get; set; }
        public string? ProductName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal TotalAmount => (Quantity ?? 0) * (UnitPrice ?? 0);
    }
}
