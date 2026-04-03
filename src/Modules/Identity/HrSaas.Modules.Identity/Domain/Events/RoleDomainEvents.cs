using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Identity.Domain.Events;

public sealed record RoleCreatedEvent(Guid TenantId, Guid RoleId, string RoleName) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record RolePermissionsChangedEvent(
    Guid TenantId,
    Guid RoleId,
    string RoleName,
    IReadOnlyList<string> NewPermissions) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
