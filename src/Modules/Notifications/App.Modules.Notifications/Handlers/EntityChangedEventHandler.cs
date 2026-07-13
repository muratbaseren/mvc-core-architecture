using App.Application.Abstractions;
using App.Application.Events;
using App.Modules.Notifications.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace App.Modules.Notifications.Handlers;

/// <summary>
/// Diğer modüllerin yayınladığı EntityChangedEvent'leri dinler ve
/// uygulama içi bildirime çevirir. Yayıncı modül bu modülü tanımaz —
/// gevşek bağlı modüller arası iletişim örneğidir.
/// </summary>
public class EntityChangedEventHandler(
    IRepository<Notification> repository,
    IUnitOfWork unitOfWork,
    ILogger<EntityChangedEventHandler> logger) : INotificationHandler<EntityChangedEvent>
{
    public async Task Handle(EntityChangedEvent notification, CancellationToken cancellationToken)
    {
        var actionText = notification.Action switch
        {
            "Created" => "oluşturuldu",
            "Updated" => "güncellendi",
            "Deleted" => "silindi",
            _ => notification.Action
        };

        await repository.AddAsync(new Notification
        {
            Title = $"{notification.EntityName} {actionText}",
            Message = $"\"{notification.DisplayName ?? notification.EntityId.ToString()}\" {actionText}."
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Bildirim oluşturuldu: {Entity} {Action} ({DisplayName})",
            notification.EntityName, notification.Action, notification.DisplayName);
    }
}
