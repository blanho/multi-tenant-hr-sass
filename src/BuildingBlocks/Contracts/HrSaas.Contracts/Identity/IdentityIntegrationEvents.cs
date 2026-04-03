namespace HrSaas.Contracts.Identity;

using HrSaas.SharedKernel.Events;

public sealed record UserRegisteredIntegrationEvent(
    Guid TenantId,
    Guid UserId,
    string Email,
    string Role) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record UserDeactivatedIntegrationEvent(
    Guid TenantId,
    Guid UserId,
    string Email) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record UserRoleChangedIntegrationEvent(
    Guid TenantId,
    Guid UserId,
    string OldRole,
    string NewRole) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
