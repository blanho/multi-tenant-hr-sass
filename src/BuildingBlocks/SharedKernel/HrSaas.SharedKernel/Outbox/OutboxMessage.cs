namespace HrSaas.SharedKernel.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Type { get; init; }
    public required string Content { get; init; }
    public Guid TenantId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    public void MarkProcessed() => ProcessedAt = DateTime.UtcNow;

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }

    public bool IsProcessed => ProcessedAt.HasValue;
    public bool HasFailed => Error is not null;
    public bool ShouldRetry => RetryCount < 5 && !IsProcessed;
}
