namespace App.Application.Abstractions;

/// <summary>
/// Bir MediatR sorgusuna uygulandığında sonucu otomatik önbelleğe alınır
/// (bkz. CachingBehavior). Handler'a dokunmak gerekmez.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>Önbellek anahtarı (örn. "products:all").</summary>
    string CacheKey { get; }

    /// <summary>Önbellekte kalma süresi. Varsayılan: 60 saniye.</summary>
    TimeSpan Duration => TimeSpan.FromSeconds(60);
}
