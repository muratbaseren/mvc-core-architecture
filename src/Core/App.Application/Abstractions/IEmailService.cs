namespace App.Application.Abstractions;

/// <summary>
/// E-posta gönderim soyutlaması. Geliştirme ortamında log'a yazan
/// implementasyon kullanılır; üretimde SMTP/SendGrid vb. ile değiştirilebilir.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
