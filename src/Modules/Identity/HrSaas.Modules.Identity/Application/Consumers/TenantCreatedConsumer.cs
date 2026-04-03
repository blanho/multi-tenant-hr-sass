using HrSaas.Contracts.Tenant;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Infrastructure.Persistence.Seed;
using HrSaas.TenantSdk;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Identity.Application.Consumers;

public sealed class TenantCreatedConsumer(
    IRoleRepository roleRepository,
    TenantContext tenantContext,
    ILogger<TenantCreatedConsumer> logger) : IConsumer<TenantCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TenantCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        tenantContext.TenantId = msg.TenantId;

        var existingRoles = await roleRepository
            .GetAllAsync(msg.TenantId, context.CancellationToken)
            .ConfigureAwait(false);

        if (existingRoles.Count > 0)
        {
            logger.LogInformation(
                "Default roles already exist for tenant {TenantId}, skipping seed",
                msg.TenantId);
            return;
        }

        var defaultRoles = DefaultRoleSeeder.CreateDefaultRoles(msg.TenantId);

        await roleRepository.AddRangeAsync(defaultRoles, context.CancellationToken).ConfigureAwait(false);
        await roleRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Seeded {RoleCount} default roles for tenant {TenantId}",
            defaultRoles.Count,
            msg.TenantId);
    }
}
