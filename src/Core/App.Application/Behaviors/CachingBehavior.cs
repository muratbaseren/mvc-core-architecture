using App.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace App.Application.Behaviors;

/// <summary>
/// MediatR önbellek davranışı:
/// - <see cref="ICacheableQuery"/> uygulayan sorguların sonucu IMemoryCache'te tutulur.
/// - <see cref="ICacheInvalidator"/> uygulayan komutlar tamamlanınca ilgili anahtarlar silinir.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>(
    IMemoryCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Sorgu önbellekleme
        if (request is ICacheableQuery cacheable)
        {
            if (cache.TryGetValue(cacheable.CacheKey, out TResponse? cached) && cached is not null)
            {
                logger.LogDebug("Önbellekten okundu: {CacheKey}", cacheable.CacheKey);
                return cached;
            }

            var response = await next(cancellationToken);
            cache.Set(cacheable.CacheKey, response, cacheable.Duration);
            logger.LogDebug("Önbelleğe yazıldı: {CacheKey} ({Duration})",
                cacheable.CacheKey, cacheable.Duration);
            return response;
        }

        // Komut sonrası önbellek temizleme
        if (request is ICacheInvalidator invalidator)
        {
            var response = await next(cancellationToken);
            foreach (var key in invalidator.CacheKeysToInvalidate)
            {
                cache.Remove(key);
                logger.LogDebug("Önbellek temizlendi: {CacheKey}", key);
            }
            return response;
        }

        return await next(cancellationToken);
    }
}
