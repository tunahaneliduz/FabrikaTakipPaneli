using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;

namespace FabrikaTakipPaneli.Pages.Products;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<string> ExistingLocations { get; set; } = new List<string>();

    private async Task LoadExistingLocationsAsync()
    {
        ExistingLocations = await _context.Products
            .Where(p => p.Location != null && p.Location != "")
            .Select(p => p.Location!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = product.Id,
            Name = product.Name,
            Unit = product.Unit,
            Description = product.Description,
            Category = product.Category,
            Location = product.Location,
            UnitPrice = product.UnitPrice,
            MinStockLevel = product.MinStockLevel
        };

        await LoadExistingLocationsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadExistingLocationsAsync();
            return Page();
        }

        var product = await _context.Products.FindAsync(Input.Id);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = Input.Name;
        product.Unit = Input.Unit;
        product.Description = Input.Description;
        product.Category = Input.Category;
        product.Location = Input.Location;
        product.UnitPrice = Input.UnitPrice;
        product.MinStockLevel = Input.MinStockLevel;

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    public class InputModel
    {
        public int Id { get; set; }

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

        [MaxLength(150)]
        [Display(Name = "Konum/Bölüm")]
        public string? Location { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat negatif olamaz.")]
        [Display(Name = "Birim Fiyat")]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Min stok eşiği negatif olamaz.")]
        [Display(Name = "Min Stok Eşiği")]
        public decimal? MinStockLevel { get; set; }
    }
}
