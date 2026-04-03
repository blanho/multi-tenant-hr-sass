namespace HrSaas.SharedKernel.Outbox;

public interface IOutboxStore
{
    Task SaveAsync(OutboxMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid messageId, CancellationToken ct = default);
    Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default);
}
