using HrSaas.Contracts.Tenant;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.TenantSdk;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Identity.Application.Consumers;

public sealed class TenantReactivatedConsumer(
    IUserRepository userRepository,
    TenantContext tenantContext,
    ILogger<TenantReactivatedConsumer> logger) : IConsumer<TenantReactivatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TenantReactivatedIntegrationEvent> context)
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
            "Reactivating {UserCount} users for reinstated tenant {TenantId}.",
            users.Count, msg.TenantId);

        foreach (var user in users.Where(u => !u.IsActive))
        {
            user.Activate();
            userRepository.Update(user);
        }

        await userRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
