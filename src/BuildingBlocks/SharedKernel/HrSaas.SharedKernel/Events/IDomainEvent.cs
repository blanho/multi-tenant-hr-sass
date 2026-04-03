using MediatR;

namespace HrSaas.SharedKernel.Events;

public interface IDomainEvent : INotification
{
    Guid TenantId { get; }

    DateTime OccurredAt { get; }
}
