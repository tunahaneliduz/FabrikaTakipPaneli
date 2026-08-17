using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Data;

namespace FabrikaTakipPaneli.Pages.Drivers;

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

    public IList<Models.Driver> Drivers { get; set; } = new List<Models.Driver>();

    public async Task OnGetAsync()
    {
        var query = _context.Drivers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(d =>
                EF.Functions.Like(d.FullName, $"%{SearchTerm}%") ||
                EF.Functions.Like(d.TCKimlikNo, $"%{SearchTerm}%"));
        }

        var totalCount = await query.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        PageIndex = Math.Clamp(PageIndex, 1, TotalPages);

        Drivers = await query
            .OrderBy(d => d.FullName)
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}
