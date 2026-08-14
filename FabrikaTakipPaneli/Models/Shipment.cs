using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FabrikaTakipPaneli.Models;

public class Shipment
{
    public int Id { get; set; }

    public int? StockEntryId { get; set; }
    public StockEntry? StockEntry { get; set; }

    [Required]
    [MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Destination { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? TruckPlate { get; set; }

    [MaxLength(50)]
    public string? TruckCapacity { get; set; }

    public bool IsFullLoad { get; set; }

    [MaxLength(150)]
    public string? DriverName { get; set; }

    [MaxLength(30)]
    public string? DriverPhone { get; set; }

    public DateTime DepartureTime { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedTravelHours { get; set; }

    [MaxLength(500)]
    public string? CertificateInfo { get; set; }

    public ShipmentStatus? ManualStatusOverride { get; set; }

    [NotMapped]
    public ShipmentStatus EffectiveStatus => ManualStatusOverride ?? GetLiveStatus(DateTime.Now);

    public ShipmentStatus GetLiveStatus(DateTime now)
    {
        var arrival = DepartureTime.AddHours((double)EstimatedTravelHours);

        if (now < arrival)
        {
            return ShipmentStatus.Yolda;
        }

        if (now < arrival.AddHours(1))
        {
            return ShipmentStatus.TeslimEdiliyor;
        }

        return ShipmentStatus.TeslimEdildi;
    }
}
