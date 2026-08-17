using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;

namespace FabrikaTakipPaneli.Pages.Vehicles;

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
    public int PageIndex { get; set; } = 1;

    public int TotalPages { get; set; }

    public IList<Models.Vehicle> Vehicles { get; set; } = new List<Models.Vehicle>();

    public async Task OnGetAsync()
    {
        var query = _context.Vehicles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(v => EF.Functions.Like(v.PlateNumber, $"%{SearchTerm}%"));
        }

        var totalCount = await query.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        PageIndex = Math.Clamp(PageIndex, 1, TotalPages);

        Vehicles = await query
            .OrderBy(v => v.PlateNumber)
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}
