using App.Application.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace App.Modules.Notifications.Services;

/// <summary>
/// MailKit tabanlı SMTP e-posta gönderimi. appsettings'te
/// "Email:Smtp:Host" tanımlıysa modül tarafından IEmailService olarak kaydedilir
/// (varsayılan LogEmailService'in yerine geçer).
///
/// Örnek yapılandırma:
///   "Email": { "Smtp": {
///     "Host": "smtp.ornek.com", "Port": 587, "UseStartTls": true,
///     "User": "kullanici", "Password": "sifre",
///     "FromAddress": "noreply@ornek.com", "FromName": "Uygulama"
///   } }
/// </summary>
public class SmtpEmailService(
    IConfiguration configuration,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var smtp = configuration.GetSection("Email:Smtp");
        var host = smtp["Host"]!;
        var port = smtp.GetValue("Port", 587);
        var useStartTls = smtp.GetValue("UseStartTls", true);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            smtp["FromName"] ?? "Uygulama",
            smtp["FromAddress"] ?? smtp["User"] ?? "noreply@localhost"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port,
            useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
            cancellationToken);

        var user = smtp["User"];
        if (!string.IsNullOrEmpty(user))
            await client.AuthenticateAsync(user, smtp["Password"] ?? string.Empty, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("E-posta gönderildi: {To} | {Subject}", to, subject);
    }
}
