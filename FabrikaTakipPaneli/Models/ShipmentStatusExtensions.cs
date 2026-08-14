namespace FabrikaTakipPaneli.Models;

public static class ShipmentStatusExtensions
{
    public static string ToDisplayLabel(this ShipmentStatus status) => status switch
    {
        ShipmentStatus.Yolda => "Yolda",
        ShipmentStatus.TeslimEdiliyor => "Teslim Ediliyor",
        ShipmentStatus.TeslimEdildi => "Teslim Edildi",
        _ => status.ToString()
    };

    public static string ToBadgeClass(this ShipmentStatus status) => status switch
    {
        ShipmentStatus.Yolda => "text-bg-warning",
        ShipmentStatus.TeslimEdiliyor => "text-bg-info",
        ShipmentStatus.TeslimEdildi => "text-bg-success",
        _ => "text-bg-secondary"
    };
}
