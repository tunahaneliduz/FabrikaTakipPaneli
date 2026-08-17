using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

    public int? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    [MaxLength(20)]
    public string? TruckPlate { get; set; }

    [MaxLength(50)]
    public string? TruckCapacity { get; set; }

    public bool IsFullLoad { get; set; }

    [MaxLength(150)]
    public string? DriverName { get; set; }

    [MaxLength(30)]
    public string? DriverPhone { get; set; }

    [NotMapped]
    public string? EffectiveDriverName => Driver?.FullName ?? DriverName;

    [NotMapped]
    public string? EffectiveDriverPhone => Driver?.Phone ?? DriverPhone;

    [NotMapped]
    public string? EffectiveTruckPlate => Vehicle?.PlateNumber ?? TruckPlate;

    [NotMapped]
    public string? EffectiveTruckCapacity => Vehicle?.Capacity ?? TruckCapacity;

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

    /// <summary>
    /// Maps the 3 tracked statuses onto the 4 visual stepper stages by splitting
    /// "Yolda" into "Hazırlanıyor" (before departure) and "Yolda" (in transit).
    /// 1=Hazırlanıyor, 2=Yolda, 3=Teslim Ediliyor, 4=Teslim Edildi.
    /// </summary>
    public int GetVisualStage(DateTime now)
    {
        if (ManualStatusOverride.HasValue)
        {
            return ManualStatusOverride.Value switch
            {
                ShipmentStatus.Yolda => 2,
                ShipmentStatus.TeslimEdiliyor => 3,
                ShipmentStatus.TeslimEdildi => 4,
                _ => 2
            };
        }

        if (now < DepartureTime)
        {
            return 1;
        }

        var arrival = DepartureTime.AddHours((double)EstimatedTravelHours);

        if (now < arrival)
        {
            return 2;
        }

        if (now < arrival.AddHours(1))
        {
            return 3;
        }

        return 4;
    }

    [NotMapped]
    public int EffectiveVisualStage => GetVisualStage(DateTime.Now);

    [NotMapped]
    public TimeSpan? EstimatedTimeRemaining
    {
        get
        {
            if (EffectiveStatus != ShipmentStatus.Yolda)
            {
                return null;
            }

            var arrival = DepartureTime.AddHours((double)EstimatedTravelHours);
            var remaining = arrival - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    [NotMapped]
    public string? EffectiveDriverPhoneTelHref
    {
        get
        {
            if (string.IsNullOrWhiteSpace(EffectiveDriverPhone))
            {
                return null;
            }

            var sanitized = new string(EffectiveDriverPhone.Where(c => char.IsDigit(c) || c == '+').ToArray());
            return sanitized.Length > 0 ? $"tel:{sanitized}" : null;
        }
    }
}
