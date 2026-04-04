using HrSaas.Modules.Storage.Domain.Entities;
using HrSaas.Modules.Storage.Domain.Enums;

namespace HrSaas.Modules.Storage.Domain.Repositories;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StoredFile>> GetByEntityAsync(string entityType, string entityId, CancellationToken ct = default);
    Task<(IReadOnlyList<StoredFile> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        FileCategory? category = null,
        FileStatus? status = null,
        string? entityType = null,
        string? entityId = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<StoredFile>> GetOrphanedFilesAsync(TimeSpan olderThan, int batchSize, CancellationToken ct = default);
    Task AddAsync(StoredFile file, CancellationToken ct = default);
    void Update(StoredFile file);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
