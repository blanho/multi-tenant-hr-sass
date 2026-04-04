namespace HrSaas.SharedKernel.Audit;

public interface IAuditContext
{
    Guid? UserId { get; }

    string? UserEmail { get; }

    Guid? TenantId { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    string? CorrelationId { get; }
}
