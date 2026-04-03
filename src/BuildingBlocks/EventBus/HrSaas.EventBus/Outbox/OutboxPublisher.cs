using System.Text.Json;
using HrSaas.SharedKernel.Events;
using HrSaas.SharedKernel.Outbox;

namespace HrSaas.EventBus.Outbox;

public sealed class OutboxPublisher(IOutboxStore store) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default) where T : IIntegrationEvent
    {
        var message = new OutboxMessage
        {
            Type = typeof(T).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(integrationEvent),
            TenantId = integrationEvent.TenantId
        };

        await store.SaveAsync(message, ct).ConfigureAwait(false);
    }
}
