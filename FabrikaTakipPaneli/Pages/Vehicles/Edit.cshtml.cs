using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;

namespace FabrikaTakipPaneli.Pages.Vehicles;

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
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = vehicle.Id,
            PlateNumber = vehicle.PlateNumber,
            Capacity = vehicle.Capacity,
            VehicleType = vehicle.VehicleType
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (await _context.Vehicles.AnyAsync(v => v.PlateNumber == Input.PlateNumber && v.Id != Input.Id))
        {
            ModelState.AddModelError("Input.PlateNumber", "Bu plaka ile kayıtlı başka bir araç zaten var.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var vehicle = await _context.Vehicles.FindAsync(Input.Id);
        if (vehicle is null)
        {
            return NotFound();
        }

        vehicle.PlateNumber = Input.PlateNumber;
        vehicle.Capacity = Input.Capacity;
        vehicle.VehicleType = Input.VehicleType;

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    public class InputModel
    {
        public int Id { get; set; }

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
