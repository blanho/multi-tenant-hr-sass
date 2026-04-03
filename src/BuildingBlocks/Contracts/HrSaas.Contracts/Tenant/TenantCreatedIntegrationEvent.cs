namespace HrSaas.Contracts.Tenant;

using HrSaas.SharedKernel.Events;

public sealed record TenantCreatedIntegrationEvent(
    Guid TenantId,
    string Name,
    string Slug,
    string ContactEmail,
    string Plan) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
