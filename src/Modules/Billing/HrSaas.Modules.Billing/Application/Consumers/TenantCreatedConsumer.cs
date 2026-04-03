using HrSaas.Contracts.Tenant;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Application.Policies;
using HrSaas.Modules.Billing.Domain.Entities;
using HrSaas.TenantSdk;
using MassTransit;

namespace HrSaas.Modules.Billing.Application.Consumers;

public sealed class TenantCreatedConsumer(
    ISubscriptionRepository subscriptionRepository,
    TenantContext tenantContext,
    IBillingPolicy billingPolicy) : IConsumer<TenantCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TenantCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        tenantContext.TenantId = msg.TenantId;

        var existing = await subscriptionRepository
            .GetActiveByTenantAsync(msg.TenantId, context.CancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return;
        }

        var planName = string.IsNullOrWhiteSpace(msg.Plan)
            ? billingPolicy.GetDefaultTrialPlan()
            : msg.Plan;

        var trialDays = billingPolicy.GetTrialDays(planName);

        var subscription = Subscription.CreateTrial(msg.TenantId, planName, trialDays);

        await subscriptionRepository.AddAsync(subscription, context.CancellationToken).ConfigureAwait(false);
        await subscriptionRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
