using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;

namespace FabrikaTakipPaneli.Pages.Drivers;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = driver.Id,
            FullName = driver.FullName,
            TCKimlikNo = driver.TCKimlikNo,
            Phone = driver.Phone,
            LicenseNumber = driver.LicenseNumber
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (await _context.Drivers.AnyAsync(d => d.TCKimlikNo == Input.TCKimlikNo && d.Id != Input.Id))
        {
            ModelState.AddModelError("Input.TCKimlikNo", "Bu TC Kimlik No ile kayıtlı başka bir sürücü zaten var.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var driver = await _context.Drivers.FindAsync(Input.Id);
        if (driver is null)
        {
            return NotFound();
        }

        driver.FullName = Input.FullName;
        driver.TCKimlikNo = Input.TCKimlikNo;
        driver.Phone = Input.Phone;
        driver.LicenseNumber = Input.LicenseNumber;

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    public class InputModel
    {
        public int Id { get; set; }

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
