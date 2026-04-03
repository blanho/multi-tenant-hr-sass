using HrSaas.Contracts.Billing;
using HrSaas.Modules.Tenant.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Tenant.Application.Consumers;

public sealed class SubscriptionCancelledConsumer(
    ITenantRepository tenantRepository,
    ILogger<SubscriptionCancelledConsumer> logger) : IConsumer<SubscriptionCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionCancelledIntegrationEvent> context)
    {
        var msg = context.Message;

        var tenant = await tenantRepository
            .GetByIdAsync(msg.TenantId, context.CancellationToken)
            .ConfigureAwait(false);

        if (tenant is null)
        {
            logger.LogWarning(
                "Tenant {TenantId} not found while handling SubscriptionCancelled for subscription {SubscriptionId}.",
                msg.TenantId, msg.SubscriptionId);
            return;
        }

        if (tenant.Status == Domain.Entities.TenantStatus.Suspended)
        {
            return;
        }

        tenant.Suspend($"Subscription cancelled: {msg.Reason}");
        tenantRepository.Update(tenant);
        await tenantRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
