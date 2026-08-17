using System.ComponentModel.DataAnnotations;

namespace FabrikaTakipPaneli.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string PlateNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Capacity { get; set; }

    [MaxLength(50)]
    public string? VehicleType { get; set; }

    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
