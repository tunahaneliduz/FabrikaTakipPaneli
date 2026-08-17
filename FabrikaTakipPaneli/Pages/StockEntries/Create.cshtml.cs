using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikaTakipPaneli.Data;
using FabrikaTakipPaneli.Models;
using FabrikaTakipPaneli.Services;

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
    public SelectList DriverOptions { get; set; } = default!;
    public SelectList VehicleOptions { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Input.EntryDate = DateTime.Now;
        await LoadProductOptionsAsync();
        await LoadDriverAndVehicleOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Type != StockEntryType.Out)
        {
            Input.AddShipment = false;
        }

        if (Input.AddShipment)
        {
            if (string.IsNullOrWhiteSpace(Input.Destination))
            {
                ModelState.AddModelError("Input.Destination", "Varış yeri zorunludur.");
            }

            if (Input.DepartureTime is null)
            {
                ModelState.AddModelError("Input.DepartureTime", "Yola çıkış zamanı zorunludur.");
            }
        }

        if (Input.Type == StockEntryType.Out && Input.ProductId > 0)
        {
            var currentStock = await _context.StockEntries
                .Where(s => s.ProductId == Input.ProductId)
                .SumAsync(s => s.Type == StockEntryType.In ? s.Quantity : -s.Quantity);

            if (Input.Quantity > currentStock)
            {
                ModelState.AddModelError("Input.Quantity",
                    $"Yetersiz stok, mevcut: {currentStock:N2}, girilmeye çalışılan: {Input.Quantity:N2}");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadProductOptionsAsync();
            await LoadDriverAndVehicleOptionsAsync();
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

        if (Input.AddShipment)
        {
            var orderNumber = await ShipmentOrderNumberGenerator.GenerateAsync(_context);

            var shipment = new Shipment
            {
                StockEntryId = stockEntry.Id,
                OrderNumber = orderNumber,
                Destination = Input.Destination!,
                DriverId = Input.DriverId,
                VehicleId = Input.VehicleId,
                IsFullLoad = Input.IsFullLoad,
                DepartureTime = Input.DepartureTime!.Value,
                EstimatedTravelHours = Input.EstimatedTravelHours ?? 0,
                CertificateInfo = Input.CertificateInfo
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("Index");
    }

    private async Task LoadProductOptionsAsync()
    {
        var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
        ProductOptions = new SelectList(products, nameof(Product.Id), nameof(Product.Name));
    }

    private async Task LoadDriverAndVehicleOptionsAsync()
    {
        var drivers = await _context.Drivers.OrderBy(d => d.FullName).ToListAsync();
        DriverOptions = new SelectList(drivers, nameof(Driver.Id), nameof(Driver.FullName));

        var vehicles = await _context.Vehicles.OrderBy(v => v.PlateNumber).ToListAsync();
        VehicleOptions = new SelectList(vehicles, nameof(Vehicle.Id), nameof(Vehicle.PlateNumber));
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

        [Display(Name = "Sevkiyat bilgisi ekle")]
        public bool AddShipment { get; set; }

        [MaxLength(300)]
        [Display(Name = "Varış Yeri")]
        public string? Destination { get; set; }

        [Display(Name = "Araç")]
        public int? VehicleId { get; set; }

        [Display(Name = "Dolu Yük")]
        public bool IsFullLoad { get; set; }

        [Display(Name = "Sürücü")]
        public int? DriverId { get; set; }

        [Display(Name = "Yola Çıkış Zamanı")]
        public DateTime? DepartureTime { get; set; }

        [Range(0, 1000, ErrorMessage = "Tahmini süre 0-1000 saat arasında olmalı.")]
        [Display(Name = "Tahmini Yolculuk Süresi (saat)")]
        public decimal? EstimatedTravelHours { get; set; }

        [MaxLength(500)]
        [Display(Name = "Sertifika Bilgisi")]
        public string? CertificateInfo { get; set; }
    }
}
