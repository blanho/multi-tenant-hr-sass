using System.Text.Json;
using HrSaas.SharedKernel.Jobs;
using HrSaas.SharedKernel.Outbox;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.EventBus.Outbox;

public sealed class OutboxProcessorJob(
    IOutboxStore outboxStore,
    IPublishEndpoint publishEndpoint,
    ILogger<OutboxProcessorJob> logger) : IRecurringJob
{
    private const int BatchSize = 50;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var messages = await outboxStore.GetUnprocessedAsync(BatchSize, ct).ConfigureAwait(false);

        if (messages.Count == 0) return;

        logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var msg in messages)
        {
            try
            {
                var type = Type.GetType(msg.Type);
                if (type is null)
                {
                    await outboxStore.MarkFailedAsync(msg.Id, $"Type not found: {msg.Type}", ct)
                        .ConfigureAwait(false);
                    continue;
                }

                var @event = JsonSerializer.Deserialize(msg.Content, type);
                if (@event is not null)
                {
                    await publishEndpoint.Publish(@event, type, ct).ConfigureAwait(false);
                }

                await outboxStore.MarkProcessedAsync(msg.Id, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to process outbox message {MessageId} of type {Type}",
                    msg.Id, msg.Type);
                await outboxStore.MarkFailedAsync(msg.Id, ex.Message, ct).ConfigureAwait(false);
            }
        }
    }
}
