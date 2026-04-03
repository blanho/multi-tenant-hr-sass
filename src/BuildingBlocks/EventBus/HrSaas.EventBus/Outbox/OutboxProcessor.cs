using System.Text.Json;
using HrSaas.SharedKernel.Events;
using HrSaas.SharedKernel.Outbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HrSaas.EventBus.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);
            await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var bus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await store.GetUnprocessedAsync(BatchSize, ct).ConfigureAwait(false);

        foreach (var msg in messages)
        {
            try
            {
                var type = Type.GetType(msg.Type);
                if (type is null)
                {
                    await store.MarkFailedAsync(msg.Id, $"Type not found: {msg.Type}", ct).ConfigureAwait(false);
                    continue;
                }

                var @event = JsonSerializer.Deserialize(msg.Content, type);
                if (@event is not null)
                {
                    await bus.Publish(@event, type, ct).ConfigureAwait(false);
                }

                await store.MarkProcessedAsync(msg.Id, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message {MessageId} of type {Type}", msg.Id, msg.Type);
                await store.MarkFailedAsync(msg.Id, ex.Message, ct).ConfigureAwait(false);
            }
        }
    }
}
