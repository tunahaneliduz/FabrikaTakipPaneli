using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.StockEntries;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ProductOptions { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var stockEntry = await _context.StockEntries.FindAsync(id);
        if (stockEntry is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = stockEntry.Id,
            ProductId = stockEntry.ProductId,
            Type = stockEntry.Type,
            Quantity = stockEntry.Quantity,
            UnitPrice = stockEntry.UnitPrice,
            EntryDate = stockEntry.EntryDate,
            Note = stockEntry.Note
        };

        await LoadProductOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadProductOptionsAsync();
            return Page();
        }

        var stockEntry = await _context.StockEntries.FindAsync(Input.Id);
        if (stockEntry is null)
        {
            return NotFound();
        }

        stockEntry.ProductId = Input.ProductId;
        stockEntry.Type = Input.Type;
        stockEntry.Quantity = Input.Quantity;
        stockEntry.UnitPrice = Input.UnitPrice;
        stockEntry.EntryDate = Input.EntryDate;
        stockEntry.Note = Input.Note;

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    private async Task LoadProductOptionsAsync()
    {
        var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
        ProductOptions = new SelectList(products, nameof(Product.Id), nameof(Product.Name));
    }

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ürün")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Hareket Tipi")]
        public StockEntryType Type { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalı.")]
        [Display(Name = "Miktar")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat negatif olamaz.")]
        [Display(Name = "Birim Fiyat")]
        public decimal? UnitPrice { get; set; }

        [Required]
        [Display(Name = "Tarih")]
        public DateTime EntryDate { get; set; }

        [MaxLength(500)]
        [Display(Name = "Not")]
        public string? Note { get; set; }
    }
}
