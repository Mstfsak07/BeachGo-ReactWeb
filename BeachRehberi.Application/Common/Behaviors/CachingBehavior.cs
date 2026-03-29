using BeachRehberi.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Common.Behaviors;

/// <summary>
/// Bu interface'i implemente eden query'ler otomatik cache'lenir.
/// </summary>
public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan? CacheExpiry { get; }
    bool BypassCache { get; }
}

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheable cacheable || cacheable.BypassCache)
            return await next();

        var cacheKey = cacheable.CacheKey;

        var cached = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit: {CacheKey}", cacheKey);
            return cached;
        }

        var response = await next();

        if (response is not null)
        {
            await _cacheService.SetAsync(cacheKey, response, cacheable.CacheExpiry, cancellationToken);
            _logger.LogDebug("Cache set: {CacheKey} (TTL: {Expiry})", cacheKey, cacheable.CacheExpiry);
        }

        return response;
    }
}
