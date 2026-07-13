using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace App.Application.Behaviors;

/// <summary>
/// MediatR pipeline'ındaki her request'i süresiyle birlikte loglar.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("İşleniyor: {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();
            logger.LogInformation(
                "Tamamlandı: {RequestName} ({ElapsedMs} ms)",
                requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "Hata: {RequestName} ({ElapsedMs} ms)",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
