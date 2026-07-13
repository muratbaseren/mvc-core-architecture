namespace App.Modules.Products.Application;

/// <summary>Ürün sorgularının önbellek anahtarları.</summary>
public static class ProductCacheKeys
{
    public const string All = "products:all";
    public static string ById(Guid id) => $"products:{id}";
}
