namespace App.Application.Abstractions;

/// <summary>
/// Bir MediatR komutuna uygulandığında, komut başarıyla tamamlandıktan sonra
/// belirtilen önbellek anahtarları temizlenir (bkz. CachingBehavior).
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>Temizlenecek önbellek anahtarları.</summary>
    IEnumerable<string> CacheKeysToInvalidate { get; }
}
