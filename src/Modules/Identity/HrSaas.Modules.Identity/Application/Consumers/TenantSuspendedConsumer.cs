using HrSaas.Contracts.Tenant;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.TenantSdk;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Identity.Application.Consumers;

public sealed class TenantSuspendedConsumer(
    IUserRepository userRepository,
    TenantContext tenantContext,
    ILogger<TenantSuspendedConsumer> logger) : IConsumer<TenantSuspendedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TenantSuspendedIntegrationEvent> context)
    {
        var msg = context.Message;
        tenantContext.TenantId = msg.TenantId;

        var users = await userRepository
            .GetAllAsync(msg.TenantId, context.CancellationToken)
            .ConfigureAwait(false);

        if (users.Count == 0)
        {
            return;
        }

        logger.LogInformation(
            "Deactivating {UserCount} users for suspended tenant {TenantId}. Reason: {Reason}",
            users.Count, msg.TenantId, msg.Reason);

        foreach (var user in users.Where(u => u.IsActive))
        {
            user.Deactivate();
            userRepository.Update(user);
        }

        await userRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
