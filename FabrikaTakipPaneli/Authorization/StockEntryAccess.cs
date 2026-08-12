using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Authorization;

public static class StockEntryAccess
{
    /// <summary>
    /// Admin her kaydı düzenleyebilir/silebilir. Personel ise sadece kendi
    /// oluşturduğu ve aynı gün içinde girdiği kayıtları düzenleyebilir/silebilir.
    /// </summary>
    public static bool CanModify(string ownerUserId, DateTime createdAt, string? currentUserId, bool isAdmin)
    {
        if (isAdmin)
        {
            return true;
        }

        return currentUserId is not null
            && ownerUserId == currentUserId
            && createdAt.Date == DateTime.Now.Date;
    }

    public static bool CanModify(StockEntry entry, string? currentUserId, bool isAdmin)
        => CanModify(entry.UserId, entry.CreatedAt, currentUserId, isAdmin);
}
