using HrSaas.Contracts.Employee;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.TenantSdk;
using MassTransit;

namespace HrSaas.Modules.Billing.Application.Consumers;

public sealed class EmployeeDeletedConsumer(
    ISubscriptionRepository subscriptionRepository,
    TenantContext tenantContext) : IConsumer<EmployeeDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EmployeeDeletedIntegrationEvent> context)
    {
        var msg = context.Message;
        tenantContext.TenantId = msg.TenantId;

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
