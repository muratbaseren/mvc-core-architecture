namespace App.Application.Abstractions;

/// <summary>
/// Dosya depolama soyutlaması. Varsayılan implementasyon yerel disktir
/// (Media modülü); S3/Azure Blob gibi sağlayıcılarla değiştirilebilir.
/// </summary>
public interface IFileStorage
{
    /// <summary>Dosyayı kaydeder ve genel erişim URL'ini (göreli) döner.</summary>
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>SaveAsync'in döndürdüğü URL ile dosyayı siler.</summary>
    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}
