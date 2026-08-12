using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.StockEntries;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ProductOptions { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Input.EntryDate = DateTime.Now;
        await LoadProductOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadProductOptionsAsync();
            return Page();
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var stockEntry = new StockEntry
        {
            ProductId = Input.ProductId,
            Type = Input.Type,
            Quantity = Input.Quantity,
            UnitPrice = Input.UnitPrice,
            EntryDate = Input.EntryDate,
            Note = Input.Note,
            UserId = userId,
            CreatedAt = DateTime.Now
        };

        _context.StockEntries.Add(stockEntry);
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
