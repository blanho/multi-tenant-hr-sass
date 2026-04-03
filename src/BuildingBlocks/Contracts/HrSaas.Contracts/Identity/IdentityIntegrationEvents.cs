namespace HrSaas.Contracts.Identity;

using HrSaas.SharedKernel.Events;

public sealed record UserRegisteredIntegrationEvent(
    Guid TenantId,
    Guid UserId,
    string Email,
    Guid RoleId,
    string RoleName) : IIntegrationEvent
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
    Guid OldRoleId,
    Guid NewRoleId) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
