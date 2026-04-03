using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;

namespace HrSaas.Api.Infrastructure.Idempotency;

public sealed class IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache, ILogger<IdempotencyMiddleware> logger)
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private static readonly TimeSpan KeyTtl = TimeSpan.FromHours(24);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) && !HttpMethods.IsPut(context.Request.Method))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var keyValues) || string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var tenantId = context.User.FindFirstValue("tenant_id") ?? "anon";
        var rawKey = keyValues.ToString();
        var cacheKey = $"idempotency:{tenantId}:{rawKey}";

        var cached = await cache.GetStringAsync(cacheKey, context.RequestAborted).ConfigureAwait(false);
        if (cached is not null)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Idempotency-Replayed"] = "true";
            await context.Response.WriteAsync(cached, context.RequestAborted).ConfigureAwait(false);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await next(context).ConfigureAwait(false);

        buffer.Position = 0;
        var responseBody = await new StreamReader(buffer).ReadToEndAsync(context.RequestAborted).ConfigureAwait(false);

        if (context.Response.StatusCode is >= 200 and < 300)
        {
            await cache.SetStringAsync(cacheKey, responseBody, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = KeyTtl }, context.RequestAborted).ConfigureAwait(false);
            logger.LogDebug("Cached idempotency response for key {Key}", rawKey);
        }

        buffer.Position = 0;
        context.Response.Body = originalBody;
        await buffer.CopyToAsync(originalBody, context.RequestAborted).ConfigureAwait(false);
    }
}
