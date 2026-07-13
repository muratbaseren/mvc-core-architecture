using MediatR;

namespace App.Application.Events;

/// <summary>
/// Modüller arası gevşek bağlı iletişim için ortak domain event'i.
/// Bir modül bu event'i IPublisher ile yayınlar; ilgilenen modüller
/// INotificationHandler ile dinler (örn. Bildirim modülü).
/// Yayıncı ile dinleyici birbirini tanımaz — biri silinse diğeri çalışmaya devam eder.
/// </summary>
public record EntityChangedEvent(
    string EntityName,
    Guid EntityId,
    string Action,
    string? DisplayName) : INotification;
