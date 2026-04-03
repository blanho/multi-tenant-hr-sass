using HrSaas.SharedKernel.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.EventBus;

public sealed class MassTransitEventBus(
    IPublishEndpoint publishEndpoint,
    ILogger<MassTransitEventBus> logger)
    : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent
    {
        logger.LogInformation(
            "Publishing integration event {EventType} | EventId: {EventId} | TenantId: {TenantId}",
            typeof(T).Name,
            integrationEvent.Id,
            integrationEvent.TenantId);

        await publishEndpoint.Publish(integrationEvent, ct).ConfigureAwait(false);
    }
}
