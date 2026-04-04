using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.SharedKernel.Behaviors;

public sealed class AuditBehavior<TRequest, TResponse>(
    IAuditContext auditContext,
    IAuditLogStore auditLogStore,
    IEntityChangeCollector changeCollector,
    ILogger<AuditBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var auditable = typeof(TRequest).GetCustomAttribute<AuditableAttribute>();
        if (auditable is null)
        {
            return await next().ConfigureAwait(false);
        }

        var sw = Stopwatch.StartNew();
        changeCollector.Clear();

        TResponse response;
        bool isSuccess;
        string? errorMessage = null;

        try
        {
            response = await next().ConfigureAwait(false);
            (isSuccess, errorMessage) = ExtractResultOutcome(response);
        }
        catch (Exception ex)
        {
            sw.Stop();
            await PersistAuditEntryAsync(
                request, auditable, false, ex.Message, sw.ElapsedMilliseconds, cancellationToken)
                .ConfigureAwait(false);
            throw;
        }

        sw.Stop();
        await PersistAuditEntryAsync(
            request, auditable, isSuccess, errorMessage, sw.ElapsedMilliseconds, cancellationToken)
            .ConfigureAwait(false);

        return response;
    }

    private async Task PersistAuditEntryAsync(
        TRequest request,
        AuditableAttribute auditable,
        bool isSuccess,
        string? errorMessage,
        long durationMs,
        CancellationToken ct)
    {
        try
        {
            var entityId = ExtractEntityId(request);
            var tenantId = ExtractTenantId(request);
            var changes = changeCollector.Collect();

            var entry = new AuditEntry
            {
                TenantId = tenantId ?? auditContext.TenantId ?? Guid.Empty,
                UserId = auditContext.UserId,
                UserEmail = auditContext.UserEmail,
                Action = auditable.Action,
                Category = auditable.Category,
                Severity = auditable.Severity,
                EntityType = ResolveEntityType(request),
                EntityId = entityId,
                CommandName = typeof(TRequest).Name,
                Description = auditable.Description,
                Payload = SerializeToDocument(request),
                OldValues = changes.Count > 0 ? changes[0].OldValues : null,
                NewValues = changes.Count > 0 ? changes[0].NewValues : null,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                IpAddress = auditContext.IpAddress,
                UserAgent = auditContext.UserAgent,
                CorrelationId = auditContext.CorrelationId,
                DurationMs = durationMs
            };

            await auditLogStore.StoreAsync(entry, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist audit entry for {CommandName}", typeof(TRequest).Name);
        }
    }

    private static (bool IsSuccess, string? Error) ExtractResultOutcome(TResponse response)
    {
        if (response is Result nonGeneric)
        {
            return (nonGeneric.IsSuccess, nonGeneric.Error);
        }

        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var isSuccess = (bool)responseType.GetProperty(nameof(Result.IsSuccess))!.GetValue(response)!;
            var error = (string?)responseType.GetProperty(nameof(Result.Error))!.GetValue(response);
            return (isSuccess, error);
        }

        return (true, null);
    }

    private static string? ExtractEntityId(TRequest request)
    {
        var idProp = typeof(TRequest).GetProperties()
            .FirstOrDefault(p =>
                p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                !p.Name.Equals("TenantId", StringComparison.OrdinalIgnoreCase) &&
                p.Name != "Id" &&
                p.PropertyType == typeof(Guid));

        if (idProp is not null)
        {
            var value = idProp.GetValue(request);
            return value is Guid g && g != Guid.Empty ? g.ToString() : null;
        }

        var genericId = typeof(TRequest).GetProperty("Id");
        if (genericId is not null)
        {
            var value = genericId.GetValue(request);
            return value?.ToString();
        }

        return null;
    }

    private static Guid? ExtractTenantId(TRequest request)
    {
        var prop = typeof(TRequest).GetProperty("TenantId");
        if (prop is not null && prop.PropertyType == typeof(Guid))
        {
            var value = (Guid)prop.GetValue(request)!;
            return value != Guid.Empty ? value : null;
        }

        return null;
    }

    private static string ResolveEntityType(TRequest request)
    {
        var name = typeof(TRequest).Name;

        var suffixes = new[] { "Command", "Query" };
        foreach (var suffix in suffixes)
        {
            if (name.EndsWith(suffix, StringComparison.Ordinal))
            {
                name = name[..^suffix.Length];
            }
        }

        var prefixes = new[] { "Create", "Update", "Delete", "Get", "Activate", "Suspend",
            "Reinstate", "Upgrade", "Cancel", "Approve", "Reject", "Apply", "Assign",
            "Send", "Retry", "Mark", "Register", "Login", "Upload", "Download", "Generate" };

        foreach (var prefix in prefixes)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name[prefix.Length..];
            }
        }

        return name;
    }

    private static JsonDocument? SerializeToDocument(TRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, SerializerOptions);
            return JsonDocument.Parse(json);
        }
        catch
        {
            return null;
        }
    }
}
