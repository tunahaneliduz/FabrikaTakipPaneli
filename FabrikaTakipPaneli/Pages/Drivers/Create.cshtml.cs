using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Drivers;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (await _context.Drivers.AnyAsync(d => d.TCKimlikNo == Input.TCKimlikNo))
        {
            ModelState.AddModelError("Input.TCKimlikNo", "Bu TC Kimlik No ile kayıtlı bir sürücü zaten var.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var driver = new Driver
        {
            FullName = Input.FullName,
            TCKimlikNo = Input.TCKimlikNo,
            Phone = Input.Phone,
            LicenseNumber = Input.LicenseNumber
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    public class InputModel
    {
        [Required]
        [MaxLength(150)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "TC Kimlik No 11 haneli rakamlardan oluşmalıdır.")]
        [Display(Name = "TC Kimlik No")]
        public string TCKimlikNo { get; set; } = string.Empty;

        [MaxLength(30)]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [MaxLength(30)]
        [Display(Name = "Ehliyet No")]
        public string? LicenseNumber { get; set; }
    }
}
