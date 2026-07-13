using App.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Services;

/// <summary>
/// E-postaları gerçekten göndermek yerine log'a yazan geliştirme implementasyonu.
/// Şifre sıfırlama bağlantıları log dosyasından/konsoldan takip edilebilir.
/// </summary>
public class LogEmailService(ILogger<LogEmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "E-POSTA (dev) => Alıcı: {To} | Konu: {Subject} | İçerik: {Body}",
            to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
