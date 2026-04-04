using HrSaas.SharedKernel.Audit;

namespace HrSaas.Modules.Audit.Application.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    string? UserEmail,
    AuditAction Action,
    AuditCategory Category,
    AuditSeverity Severity,
    string EntityType,
    string? EntityId,
    string CommandName,
    string? Description,
    bool IsSuccess,
    string? ErrorMessage,
    string? IpAddress,
    long DurationMs,
    DateTime Timestamp);

public sealed record AuditLogDetailDto(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    string? UserEmail,
    AuditAction Action,
    AuditCategory Category,
    AuditSeverity Severity,
    string EntityType,
    string? EntityId,
    string CommandName,
    string? Description,
    string? Payload,
    string? OldValues,
    string? NewValues,
    bool IsSuccess,
    string? ErrorMessage,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    long DurationMs,
    DateTime Timestamp);
