using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Identity.Domain.Events;

public sealed record UserRegisteredEvent(Guid TenantId, Guid UserId, string Email) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record UserDeactivatedEvent(Guid TenantId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record UserRoleChangedEvent(Guid TenantId, Guid UserId, string OldRole, string NewRole) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
