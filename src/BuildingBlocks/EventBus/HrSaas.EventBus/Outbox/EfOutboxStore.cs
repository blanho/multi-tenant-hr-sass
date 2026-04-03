using HrSaas.SharedKernel.Outbox;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.EventBus.Outbox;

public sealed class EfOutboxStore<TContext>(TContext dbContext) : IOutboxStore
    where TContext : DbContext
{
    public async Task SaveAsync(OutboxMessage message, CancellationToken ct = default)
    {
        await dbContext.Set<OutboxMessage>().AddAsync(message, ct).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default)
    {
        return await dbContext.Set<OutboxMessage>()
            .Where(m => !m.IsProcessed && m.ShouldRetry)
            .OrderBy(m => m.OccurredAt)
            .Take(batchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task MarkProcessedAsync(Guid messageId, CancellationToken ct = default)
    {
        var message = await dbContext.Set<OutboxMessage>()
            .FindAsync([messageId], ct).ConfigureAwait(false);

        if (message is not null)
        {
            message.MarkProcessed();
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }

    public async Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default)
    {
        var message = await dbContext.Set<OutboxMessage>()
            .FindAsync([messageId], ct).ConfigureAwait(false);

        if (message is not null)
        {
            message.MarkFailed(error);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }
}
