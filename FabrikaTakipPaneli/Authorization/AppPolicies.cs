namespace FabrikaTakipPaneli.Authorization;

public static class AppPolicies
{
    /// <summary>Admin role only.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Admin veya Personel - ürünleri görüntüleyebilir.</summary>
    public const string ViewProducts = "ViewProducts";

    /// <summary>Sadece Admin - ürün oluşturma/düzenleme/silme.</summary>
    public const string ManageProducts = "ManageProducts";

    /// <summary>Admin veya Personel - stok hareketi girebilir ve listeleyebilir.</summary>
    public const string EnterStock = "EnterStock";

    /// <summary>Sadece Admin - stok hareketi düzenleme/silme.</summary>
    public const string ManageStockEntries = "ManageStockEntries";
}
