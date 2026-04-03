namespace HrSaas.SharedKernel.Events;

public interface IIntegrationEvent
{
    Guid Id { get; }

    Guid TenantId { get; }

    DateTime OccurredAt { get; }
}
