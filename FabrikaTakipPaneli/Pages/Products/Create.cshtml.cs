using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Products;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var product = new Product
        {
            Name = Input.Name,
            Unit = Input.Unit,
            Description = Input.Description,
            Category = Input.Category,
            UnitPrice = Input.UnitPrice,
            MinStockLevel = Input.MinStockLevel
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    public class InputModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Ad")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Birim")]
        public string Unit { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [MaxLength(100)]
        [Display(Name = "Kategori")]
        public string? Category { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat negatif olamaz.")]
        [Display(Name = "Birim Fiyat")]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Min stok eşiği negatif olamaz.")]
        [Display(Name = "Min Stok Eşiği")]
        public decimal? MinStockLevel { get; set; }
    }
}
