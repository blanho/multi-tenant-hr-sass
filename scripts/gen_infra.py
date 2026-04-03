#!/usr/bin/env python3
"""Generates all principal-engineer infrastructure files."""
import os

ROOT = "/Users/macbook/Desktop/multi-tenant-sass"

def write(rel_path, content):
    full = os.path.join(ROOT, rel_path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  {rel_path}")

# ─── Outbox ───────────────────────────────────────────────────────────────────
write("src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/Outbox/OutboxMessage.cs", """\
namespace HrSaas.SharedKernel.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Type { get; init; }
    public required string Content { get; init; }
    public Guid TenantId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    public void MarkProcessed() => ProcessedAt = DateTime.UtcNow;

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }

    public bool IsProcessed => ProcessedAt.HasValue;
    public bool HasFailed => Error is not null;
    public bool ShouldRetry => RetryCount < 5 && !IsProcessed;
}
""")

write("src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/Outbox/IOutboxStore.cs", """\
namespace HrSaas.SharedKernel.Outbox;

public interface IOutboxStore
{
    Task SaveAsync(OutboxMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid messageId, CancellationToken ct = default);
    Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default);
}
""")

write("src/BuildingBlocks/EventBus/HrSaas.EventBus/Outbox/OutboxPublisher.cs", """\
using System.Text.Json;
using HrSaas.SharedKernel.Events;
using HrSaas.SharedKernel.Outbox;

namespace HrSaas.EventBus.Outbox;

public sealed class OutboxPublisher(IOutboxStore store) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default) where T : IIntegrationEvent
    {
        var message = new OutboxMessage
        {
            Type = typeof(T).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(integrationEvent),
            TenantId = integrationEvent.TenantId
        };

        await store.SaveAsync(message, ct).ConfigureAwait(false);
    }
}
""")

write("src/BuildingBlocks/EventBus/HrSaas.EventBus/Outbox/OutboxProcessor.cs", """\
using System.Text.Json;
using HrSaas.SharedKernel.Events;
using HrSaas.SharedKernel.Outbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HrSaas.EventBus.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);
            await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var bus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await store.GetUnprocessedAsync(BatchSize, ct).ConfigureAwait(false);

        foreach (var msg in messages)
        {
            try
            {
                var type = Type.GetType(msg.Type);
                if (type is null)
                {
                    await store.MarkFailedAsync(msg.Id, $"Type not found: {msg.Type}", ct).ConfigureAwait(false);
                    continue;
                }

                var @event = JsonSerializer.Deserialize(msg.Content, type);
                if (@event is not null)
                {
                    await bus.Publish(@event, type, ct).ConfigureAwait(false);
                }

                await store.MarkProcessedAsync(msg.Id, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message {MessageId} of type {Type}", msg.Id, msg.Type);
                await store.MarkFailedAsync(msg.Id, ex.Message, ct).ConfigureAwait(false);
            }
        }
    }
}
""")

print("Outbox done")

# ─── EF Core Interceptors ─────────────────────────────────────────────────────
write("src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/Interceptors/AuditableEntityInterceptor.cs", """\
using HrSaas.SharedKernel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HrSaas.SharedKernel.Interceptors;

public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateTimestamps(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Touch();
            }
        }
    }
}
""")

write("src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/Interceptors/DomainEventDispatcherInterceptor.cs", """\
using HrSaas.SharedKernel.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HrSaas.SharedKernel.Interceptors;

public sealed class DomainEventDispatcherInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context, cancellationToken).ConfigureAwait(false);
        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task DispatchDomainEventsAsync(DbContext? context, CancellationToken ct)
    {
        if (context is null)
        {
            return;
        }

        var entities = context.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var events = entities.SelectMany(e => e.DomainEvents).ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in events)
        {
            await publisher.Publish(domainEvent, ct).ConfigureAwait(false);
        }
    }
}
""")

print("Interceptors done")

# ─── OpenTelemetry ────────────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/Infrastructure/Observability/TelemetryExtensions.cs", """\
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HrSaas.Api.Infrastructure.Observability;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["Telemetry:ServiceName"] ?? "HrSaas.Api";
        var otlpEndpoint = configuration["Telemetry:OtlpEndpoint"] ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource(HrSaasActivitySource.SourceName)
                .AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint)));

        return services;
    }
}

public static class HrSaasActivitySource
{
    public const string SourceName = "HrSaas";
    private static readonly System.Diagnostics.ActivitySource Source = new(SourceName);

    public static System.Diagnostics.Activity? StartActivity(string name, System.Diagnostics.ActivityKind kind = System.Diagnostics.ActivityKind.Internal) =>
        Source.StartActivity(name, kind);
}
""")

print("Telemetry done")

# ─── Resilience ───────────────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/Infrastructure/Resilience/ResilienceExtensions.cs", """\
using Microsoft.Extensions.Http.Resilience;

namespace HrSaas.Api.Infrastructure.Resilience;

public static class ResilienceExtensions
{
    public static IHttpClientBuilder AddStandardResilience(this IHttpClientBuilder builder) =>
        builder.AddStandardResilienceHandler(opts =>
        {
            opts.Retry.MaxRetryAttempts = 3;
            opts.Retry.Delay = TimeSpan.FromMilliseconds(200);
            opts.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            opts.CircuitBreaker.FailureRatio = 0.5;
            opts.CircuitBreaker.MinimumThroughput = 10;
            opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });
}
""")

print("Resilience done")

# ─── Rate limiting ────────────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/Infrastructure/RateLimiting/TenantRateLimitingExtensions.cs", """\
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace HrSaas.Api.Infrastructure.RateLimiting;

public static class TenantRateLimitingExtensions
{
    public const string ApiPolicy = "api";
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddTenantRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opts.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { title = "Too Many Requests", status = 429, detail = "Rate limit exceeded. Please retry after 60 seconds." },
                    ct).ConfigureAwait(false);
            };

            opts.AddPolicy(ApiPolicy, context =>
            {
                var tenantId = context.User.FindFirstValue("tenant_id") ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                return RateLimitPartition.GetSlidingWindowLimiter(tenantId, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 500,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                });
            });

            opts.AddPolicy(AuthPolicy, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
        });

        return services;
    }
}
""")

print("Rate limiting done")

# ─── Idempotency ──────────────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/Infrastructure/Idempotency/IdempotencyMiddleware.cs", """\
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
""")

print("Idempotency done")

# ─── Health checks ────────────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/Infrastructure/HealthChecks/HealthCheckExtensions.cs", """\
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HrSaas.Api.Infrastructure.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddModuleHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var rabbitMq = configuration.GetConnectionString("RabbitMQ")!;
        var redis = configuration.GetConnectionString("Redis")!;

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: ["db", "readiness"])
            .AddRabbitMQ(rabbitMq, name: "rabbitmq", tags: ["messaging", "readiness"])
            .AddRedis(redis, name: "redis", tags: ["cache", "readiness"]);

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = (ctx, _) =>
            {
                ctx.Response.ContentType = "text/plain";
                return ctx.Response.WriteAsync("healthy");
            }
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Contains("readiness"),
            ResponseWriter = HealthCheckUiResponseWriter
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckUiResponseWriter
        });

        return app;
    }

    private static readonly Func<HttpContext, HealthReport, Task> HealthCheckUiResponseWriter =
        HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse;
}
""")

print("Health checks done")

# ─── API versioning ───────────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/Infrastructure/Versioning/VersioningExtensions.cs", """\
using Asp.Versioning;

namespace HrSaas.Api.Infrastructure.Versioning;

public static class VersioningExtensions
{
    public static IServiceCollection AddApiVersioningSetup(this IServiceCollection services)
    {
        services.AddApiVersioning(opts =>
        {
            opts.DefaultApiVersion = new ApiVersion(1, 0);
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.ReportApiVersions = true;
            opts.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        })
        .AddApiExplorer(opts =>
        {
            opts.GroupNameFormat = "'v'VVV";
            opts.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
""")

print("Versioning done")

# ─── Controllers ─────────────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/Controllers/AuthController.cs", """\
using Asp.Versioning;
using HrSaas.Modules.Identity.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[EnableRateLimiting("auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "EMAIL_TAKEN" ? Conflict(result.Error) : BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetMe), null, result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(result.Error);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMe()
    {
        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
        return Ok(claims);
    }
}
""")

write("src/Api/HrSaas.Api/Controllers/TenantsController.cs", """\
using Asp.Versioning;
using HrSaas.Modules.Tenant.Application.Commands;
using HrSaas.Modules.Tenant.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("api")]
public sealed class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{tenantId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(tenantId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllTenantsQuery(), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "SLUG_TAKEN" ? Conflict(result.Error) : BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { tenantId = result.Value }, new { id = result.Value });
    }

    [HttpPost("{tenantId:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid tenantId, [FromBody] SuspendRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuspendTenantCommand(tenantId, request.Reason), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}

public sealed record SuspendRequest(string Reason);
""")

write("src/Api/HrSaas.Api/Controllers/LeaveController.cs", """\
using Asp.Versioning;
using HrSaas.Modules.Leave.Application.Commands;
using HrSaas.Modules.Leave.Application.Queries;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class LeaveController(IMediator mediator, ITenantService tenantService) : ControllerBase
{
    [HttpGet("employee/{employeeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetLeavesByEmployeeQuery(tenantId, employeeId), ct);
        return Ok(result.Value);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetPendingLeavesQuery(tenantId), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apply([FromBody] ApplyLeaveCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetByEmployee), new { employeeId = command.EmployeeId }, new { id = result.Value });
    }

    [HttpPost("{leaveId:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid leaveId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new ApproveLeaveCommand(tenantId, leaveId, userId), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpPost("{leaveId:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid leaveId, [FromBody] RejectRequest request, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new RejectLeaveCommand(tenantId, leaveId, userId, request.Note), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}

public sealed record RejectRequest(string Note);
""")

write("src/Api/HrSaas.Api/Controllers/BillingController.cs", """\
using Asp.Versioning;
using HrSaas.Modules.Billing.Application.Commands;
using HrSaas.Modules.Billing.Application.Queries;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class BillingController(IMediator mediator, ITenantService tenantService) : ControllerBase
{
    [HttpGet("subscription")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscription(CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetSubscriptionByTenantQuery(tenantId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("subscription/cancel")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel([FromBody] CancelRequest request, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new CancelSubscriptionCommand(tenantId, request.SubscriptionId, request.Reason), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}

public sealed record CancelRequest(Guid SubscriptionId, string Reason);
""")

print("Controllers done")

# ─── YARP Gateway ─────────────────────────────────────────────────────────────
write("src/Gateway/HrSaas.Gateway/Program.cs", """\
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console());

    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddAuthentication().AddJwtBearer();
    builder.Services.AddAuthorization();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapReverseProxy();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway failed to start.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
""")

write("src/Gateway/HrSaas.Gateway/appsettings.json", """\
{
  "Serilog": {
    "MinimumLevel": { "Default": "Information" }
  },
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "Authority": "https://localhost:5001",
        "ValidAudiences": ["hrsaas-api"]
      }
    }
  },
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/api/v1/auth/{**catch-all}" },
        "Transforms": [{ "RequestHeader": "X-Forwarded-By", "Set": "hrsaas-gateway" }]
      },
      "billing-route": {
        "ClusterId": "billing-cluster",
        "Match": { "Path": "/api/v1/billing/{**catch-all}" },
        "AuthorizationPolicy": "default",
        "Transforms": [{ "RequestHeader": "X-Forwarded-By", "Set": "hrsaas-gateway" }]
      },
      "api-route": {
        "ClusterId": "api-cluster",
        "Match": { "Path": "/api/{**catch-all}" },
        "AuthorizationPolicy": "default",
        "Transforms": [{ "RequestHeader": "X-Forwarded-By", "Set": "hrsaas-gateway" }]
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "identity/destination1": { "Address": "http://hrsaas-api:8080" }
        },
        "HealthCheck": {
          "Active": { "Enabled": true, "Interval": "00:00:10", "Path": "/health/live" }
        }
      },
      "billing-cluster": {
        "Destinations": {
          "billing/destination1": { "Address": "http://hrsaas-api:8080" }
        }
      },
      "api-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "api/destination1": { "Address": "http://hrsaas-api:8080" }
        },
        "HealthCheck": {
          "Active": { "Enabled": true, "Interval": "00:00:10", "Path": "/health/live" }
        }
      }
    }
  }
}
""")

print("Gateway done")

# ─── Unit Tests ──────────────────────────────────────────────────────────────
write("tests/HrSaas.Modules.Employee.UnitTests/Domain/EmployeeAggregateTests.cs", """\
using Bogus;
using FluentAssertions;
using HrSaas.Modules.Employee.Domain.Entities;
using HrSaas.Modules.Employee.Domain.Events;
using HrSaas.Modules.Employee.Domain.ValueObjects;

namespace HrSaas.Modules.Employee.UnitTests.Domain;

public sealed class EmployeeAggregateTests
{
    private static readonly Faker Faker = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateEmployee()
    {
        var department = Department.Create(Faker.Commerce.Department());
        var position = Position.Create(Faker.Name.JobTitle());

        var employee = Employee.Create(_tenantId, Faker.Name.FullName(), Faker.Internet.Email(), department, position);

        employee.Should().NotBeNull();
        employee.TenantId.Should().Be(_tenantId);
        employee.IsDeleted.Should().BeFalse();
        employee.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldRaiseEmployeeCreatedEvent()
    {
        var department = Department.Create("Engineering");
        var position = Position.Create("Developer");

        var employee = Employee.Create(_tenantId, "John Doe", "john@example.com", department, position);

        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var department = Department.Create("Engineering");
        var position = Position.Create("Developer");

        var act = () => Employee.Create(_tenantId, string.Empty, "john@example.com", department, position);

        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrow()
    {
        var department = Department.Create("Engineering");
        var position = Position.Create("Developer");

        var act = () => Employee.Create(Guid.Empty, "John Doe", "john@example.com", department, position);

        act.Should().Throw<ArgumentException>().WithMessage("*tenantId*");
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        var department = Department.Create("Engineering");
        var position = Position.Create("Developer");
        var employee = Employee.Create(_tenantId, "John Doe", "john@example.com", department, position);
        employee.ClearDomainEvents();

        employee.Delete();

        employee.IsDeleted.Should().BeTrue();
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeDeletedEvent);
    }

    [Fact]
    public void Update_ShouldRaiseUpdatedEvent()
    {
        var department = Department.Create("Engineering");
        var position = Position.Create("Developer");
        var employee = Employee.Create(_tenantId, "John Doe", "john@example.com", department, position);
        employee.ClearDomainEvents();

        var newDept = Department.Create("Product");
        var newPos = Position.Create("PM");
        employee.Update("Jane Doe", department, newPos);

        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeUpdatedEvent);
    }
}
""")

write("tests/HrSaas.Modules.Employee.UnitTests/Domain/EmployeeValueObjectTests.cs", """\
using FluentAssertions;
using HrSaas.Modules.Employee.Domain.ValueObjects;

namespace HrSaas.Modules.Employee.UnitTests.Domain;

public sealed class EmployeeValueObjectTests
{
    [Theory]
    [InlineData("Engineering")]
    [InlineData("Human Resources")]
    [InlineData("Finance")]
    public void Department_Create_WithValidName_ShouldSucceed(string name)
    {
        var dept = Department.Create(name);
        dept.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Department_Create_WithInvalidName_ShouldThrow(string? name)
    {
        var act = () => Department.Create(name!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Department_SameValues_ShouldBeEqual()
    {
        var d1 = Department.Create("Engineering");
        var d2 = Department.Create("Engineering");
        d1.Should().Be(d2);
    }

    [Fact]
    public void Department_DifferentValues_ShouldNotBeEqual()
    {
        var d1 = Department.Create("Engineering");
        var d2 = Department.Create("Finance");
        d1.Should().NotBe(d2);
    }
}
""")

write("tests/HrSaas.Modules.Employee.UnitTests/Application/CreateEmployeeCommandHandlerTests.cs", """\
using FluentAssertions;
using HrSaas.Modules.Employee.Application.Commands;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.Modules.Employee.Domain.Entities;
using HrSaas.Modules.Employee.Domain.ValueObjects;
using NSubstitute;

namespace HrSaas.Modules.Employee.UnitTests.Application;

public sealed class CreateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _repository = Substitute.For<IEmployeeRepository>();
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _handler = new CreateEmployeeCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnId()
    {
        var tenantId = Guid.NewGuid();
        var command = new CreateEmployeeCommand(tenantId, "Jane Doe", "jane@example.com", "Engineering", "Developer");

        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _repository.Received(1).AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
""")

print("Unit tests done")

# ─── Integration test base ────────────────────────────────────────────────────
write("tests/HrSaas.IntegrationTests/Infrastructure/PostgresFixture.cs", """\
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace HrSaas.IntegrationTests.Infrastructure;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("hrsaas_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync() => await _container.StartAsync();
    public async Task DisposeAsync() => await _container.DisposeAsync();
}
""")

write("tests/HrSaas.IntegrationTests/Infrastructure/HrSaasWebAppFactory.cs", """\
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.IntegrationTests.Infrastructure;

public sealed class HrSaasWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.Configure<Microsoft.Extensions.Hosting.HostOptions>(opts =>
                opts.BackgroundServiceExceptionBehavior = Microsoft.Extensions.Hosting.BackgroundServiceExceptionBehavior.Ignore);
        });

        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.ConnectionString);
    }

    public async Task InitializeAsync() => await _postgres.InitializeAsync();
    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
""")

write("tests/HrSaas.IntegrationTests/Employees/EmployeeApiTests.cs", """\
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HrSaas.IntegrationTests.Infrastructure;

namespace HrSaas.IntegrationTests.Employees;

public sealed class EmployeeApiTests(HrSaasWebAppFactory factory) : IClassFixture<HrSaasWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetEmployee_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/employees/00000000-0000-0000-0000-000000000001");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllEmployees_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/employees");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
""")

print("Integration tests done")

# ─── Updated Program.cs ───────────────────────────────────────────────────────
program_cs = """\
using HrSaas.Api.Infrastructure.HealthChecks;
using HrSaas.Api.Infrastructure.Idempotency;
using HrSaas.Api.Infrastructure.Observability;
using HrSaas.Api.Infrastructure.RateLimiting;
using HrSaas.Api.Infrastructure.Versioning;
using HrSaas.Modules.Billing;
using HrSaas.Modules.Employee;
using HrSaas.Modules.Identity;
using HrSaas.Modules.Leave;
using HrSaas.Modules.Tenant;
using HrSaas.SharedKernel.Behaviors;
using HrSaas.SharedKernel.Interceptors;
using HrSaas.TenantSdk;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId());

    builder.Services.AddTelemetry(builder.Configuration);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.Authority = builder.Configuration["Jwt:Authority"];
            opts.Audience = builder.Configuration["Jwt:Audience"];
            opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        });

    builder.Services.AddAuthorization();

    builder.Services.AddTenantSdk();

    builder.Services.AddScoped<AuditableEntityInterceptor>();
    builder.Services.AddScoped<DomainEventDispatcherInterceptor>();

    builder.Services.AddEmployeeModule(builder.Configuration);
    builder.Services.AddIdentityModule(builder.Configuration);
    builder.Services.AddTenantModule(builder.Configuration);
    builder.Services.AddLeaveModule(builder.Configuration);
    builder.Services.AddBillingModule(builder.Configuration);

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    builder.Services.AddStackExchangeRedisCache(opts =>
        opts.Configuration = builder.Configuration.GetConnectionString("Redis"));

    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
            cfg.ConfigureEndpoints(ctx);
        });
    });

    builder.Services.AddApiVersioningSetup();
    builder.Services.AddTenantRateLimiting();
    builder.Services.AddModuleHealthChecks(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseMiddleware<HrSaas.Api.Middleware.ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseMiddleware<TenantMiddleware>();
    app.UseMiddleware<IdempotencyMiddleware>();

    app.MapControllers().RequireRateLimiting("api");
    app.MapHealthCheckEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
"""

full_path = os.path.join(ROOT, "src/Api/HrSaas.Api/Program.cs")
with open(full_path, "w", encoding="utf-8") as f:
    f.write(program_cs)
print("  src/Api/HrSaas.Api/Program.cs")

print("\nAll principal-engineer infrastructure files generated.")
