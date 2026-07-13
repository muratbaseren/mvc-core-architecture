namespace App.SharedKernel.Domain;

/// <summary>
/// Tüm entity'ler için temel sınıf. Id ve denetim (audit) alanlarını içerir.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
