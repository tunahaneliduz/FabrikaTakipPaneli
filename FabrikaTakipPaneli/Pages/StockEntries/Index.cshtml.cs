using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Authorization;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.StockEntries;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    private const int PageSize = 10;

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public int TotalPages { get; set; }

    public IList<StockEntryListItem> StockEntries { get; set; } = new List<StockEntryListItem>();

    private IQueryable<StockEntry> BuildFilteredQuery()
    {
        var query = _context.StockEntries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(s => EF.Functions.Like(s.Product!.Name, $"%{SearchTerm}%"));
        }

        if (StartDate.HasValue)
        {
            query = query.Where(s => s.EntryDate >= StartDate.Value.Date);
        }

        if (EndDate.HasValue)
        {
            var exclusiveEnd = EndDate.Value.Date.AddDays(1);
            query = query.Where(s => s.EntryDate < exclusiveEnd);
        }

        return query;
    }

    public async Task OnGetAsync()
    {
        var currentUserId = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole(AppRoles.Admin);

        var query = BuildFilteredQuery();

        var totalCount = await query.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        PageIndex = Math.Clamp(PageIndex, 1, TotalPages);

        var entries = await query
            .OrderByDescending(s => s.EntryDate)
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
            .Select(s => new
            {
                s.Id,
                ProductName = s.Product!.Name,
                s.Type,
                s.Quantity,
                s.UnitPrice,
                s.EntryDate,
                s.CreatedAt,
                s.UserId,
                UserName = s.User!.UserName,
                s.Note
            })
            .ToListAsync();

        StockEntries = entries.Select(s => new StockEntryListItem
        {
            Id = s.Id,
            ProductName = s.ProductName,
            Type = s.Type,
            Quantity = s.Quantity,
            UnitPrice = s.UnitPrice,
            EntryDate = s.EntryDate,
            UserName = s.UserName,
            Note = s.Note,
            IsOwnEntry = s.UserId == currentUserId,
            CanModify = StockEntryAccess.CanModify(s.UserId, s.CreatedAt, currentUserId, isAdmin)
        }).ToList();
    }

    public async Task<IActionResult> OnGetExportExcelAsync()
    {
        var entries = await BuildFilteredQuery()
            .OrderByDescending(s => s.EntryDate)
            .Select(s => new
            {
                ProductName = s.Product!.Name,
                s.Type,
                s.Quantity,
                s.UnitPrice,
                s.EntryDate,
                UserName = s.User!.UserName,
                s.Note
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stok Hareketleri");

        var headers = new[] { "Ürün", "Tip", "Miktar", "Birim Fiyat", "Tarih", "Kullanıcı", "Not" };
        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var entry in entries)
        {
            worksheet.Cell(row, 1).Value = entry.ProductName;
            worksheet.Cell(row, 2).Value = entry.Type == StockEntryType.In ? "Giriş" : "Çıkış";
            worksheet.Cell(row, 3).Value = entry.Quantity;
            worksheet.Cell(row, 4).Value = entry.UnitPrice;
            worksheet.Cell(row, 5).Value = entry.EntryDate;
            worksheet.Cell(row, 5).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
            worksheet.Cell(row, 6).Value = entry.UserName;
            worksheet.Cell(row, 7).Value = entry.Note;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = $"StokHareketleri_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
        public bool IsOwnEntry { get; set; }
        public bool CanModify { get; set; }
    }
}
