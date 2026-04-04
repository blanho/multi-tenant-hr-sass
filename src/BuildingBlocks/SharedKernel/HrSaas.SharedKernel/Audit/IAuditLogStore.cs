namespace HrSaas.SharedKernel.Audit;

public interface IAuditLogStore
{
    Task StoreAsync(AuditEntry entry, CancellationToken ct = default);

    Task StoreBatchAsync(IReadOnlyList<AuditEntry> entries, CancellationToken ct = default);
}
