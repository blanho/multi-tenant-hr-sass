using HrSaas.Contracts.Employee;
using HrSaas.Modules.Billing.Application.Interfaces;
using MassTransit;

namespace HrSaas.Modules.Billing.Application.Consumers;

public sealed class EmployeeDeletedConsumer(
    ISubscriptionRepository subscriptionRepository) : IConsumer<EmployeeDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EmployeeDeletedIntegrationEvent> context)
    {
        var msg = context.Message;

        var subscription = await subscriptionRepository
            .GetActiveByTenantAsync(msg.TenantId, context.CancellationToken)
            .ConfigureAwait(false);

        if (subscription is null)
        {
            return;
        }

        subscription.DecrementSeats();
        subscriptionRepository.Update(subscription);
        await subscriptionRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
