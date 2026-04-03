using HrSaas.Contracts.Employee;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.TenantSdk;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Billing.Application.Consumers;

public sealed class EmployeeCreatedConsumer(
    ISubscriptionRepository subscriptionRepository,
    TenantContext tenantContext,
    ILogger<EmployeeCreatedConsumer> logger) : IConsumer<EmployeeCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EmployeeCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        tenantContext.TenantId = msg.TenantId;

        var subscription = await subscriptionRepository
            .GetActiveByTenantAsync(msg.TenantId, context.CancellationToken)
            .ConfigureAwait(false);

        if (subscription is null)
        {
            logger.LogWarning(
                "No active subscription found for tenant {TenantId}; seat increment skipped for employee {EmployeeId}.",
                msg.TenantId, msg.EmployeeId);
            return;
        }

        if (!subscription.CanAddSeat())
        {
            logger.LogWarning(
                "Seat limit {MaxSeats} reached on plan {Plan} for tenant {TenantId}. Employee {EmployeeId} created over limit.",
                subscription.MaxSeats, subscription.PlanName, msg.TenantId, msg.EmployeeId);
            return;
        }

        subscription.IncrementSeats();
        subscriptionRepository.Update(subscription);
        await subscriptionRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
