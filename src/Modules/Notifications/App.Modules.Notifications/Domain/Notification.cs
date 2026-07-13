using App.SharedKernel.Domain;

namespace App.Modules.Notifications.Domain;

/// <summary>
/// Uygulama içi bildirim. GraphQL'de `notifications` sorgusu otomatik oluşur.
/// </summary>
public class Notification : BaseEntity, IAggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool IsRead { get; set; }
}
