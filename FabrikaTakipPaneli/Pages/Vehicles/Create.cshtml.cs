using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Pages.Vehicles;

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
        if (await _context.Vehicles.AnyAsync(v => v.PlateNumber == Input.PlateNumber))
        {
            ModelState.AddModelError("Input.PlateNumber", "Bu plaka ile kayıtlı bir araç zaten var.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var vehicle = new Vehicle
        {
            PlateNumber = Input.PlateNumber,
            Capacity = Input.Capacity,
            VehicleType = Input.VehicleType
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    public class InputModel
    {
        [Required]
        [MaxLength(20)]
        [Display(Name = "Plaka")]
        public string PlateNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Kapasite")]
        public string? Capacity { get; set; }

        [MaxLength(50)]
        [Display(Name = "Araç Tipi")]
        public string? VehicleType { get; set; }
    }
}
