using App.SharedKernel.Domain;

namespace App.Modules.Products.Domain;

/// <summary>
/// Ürün aggregate root'u. <see cref="IAggregateRoot"/> sayesinde
/// EF Core modeline ve GraphQL şemasına otomatik dahil olur.
/// </summary>
public class Product : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
}
