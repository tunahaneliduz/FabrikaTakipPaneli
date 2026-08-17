using System.ComponentModel.DataAnnotations;

namespace FabrikaTakipPaneli.Models;

public class Driver
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(11)]
    [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "TC Kimlik No 11 haneli rakamlardan oluşmalıdır.")]
    public string TCKimlikNo { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(30)]
    public string? LicenseNumber { get; set; }

    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
