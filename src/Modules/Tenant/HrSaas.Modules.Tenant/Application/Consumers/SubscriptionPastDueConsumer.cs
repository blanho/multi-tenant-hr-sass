using HrSaas.Contracts.Billing;
using HrSaas.Modules.Tenant.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Tenant.Application.Consumers;

public sealed class SubscriptionPastDueConsumer(
    ITenantRepository tenantRepository,
    ILogger<SubscriptionPastDueConsumer> logger) : IConsumer<SubscriptionPastDueIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionPastDueIntegrationEvent> context)
    {
        var msg = context.Message;

        var tenant = await tenantRepository
            .GetByIdAsync(msg.TenantId, context.CancellationToken)
            .ConfigureAwait(false);

        if (tenant is null)
        {
            logger.LogWarning(
                "Tenant {TenantId} not found while handling SubscriptionPastDue for subscription {SubscriptionId}.",
                msg.TenantId, msg.SubscriptionId);
            return;
        }

        if (tenant.Status == Domain.Entities.TenantStatus.Suspended)
        {
            return;
        }

        tenant.Suspend("Payment past due — subscription suspended pending payment.");
        tenantRepository.Update(tenant);
        await tenantRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
