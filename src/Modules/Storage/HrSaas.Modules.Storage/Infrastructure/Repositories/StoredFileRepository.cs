using HrSaas.Modules.Storage.Domain.Entities;
using HrSaas.Modules.Storage.Domain.Enums;
using HrSaas.Modules.Storage.Domain.Repositories;
using HrSaas.Modules.Storage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Storage.Infrastructure.Repositories;

public sealed class StoredFileRepository(StorageDbContext dbContext) : IStoredFileRepository
{
    public async Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.StoredFiles.FirstOrDefaultAsync(f => f.Id == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<StoredFile>> GetByEntityAsync(
        string entityType, string entityId, CancellationToken ct = default)
        => await dbContext.StoredFiles
            .Where(f => f.EntityType == entityType && f.EntityId == entityId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<(IReadOnlyList<StoredFile> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        FileCategory? category = null,
        FileStatus? status = null,
        string? entityType = null,
        string? entityId = null,
        CancellationToken ct = default)
    {
        var query = dbContext.StoredFiles.AsQueryable();

        if (category.HasValue)
            query = query.Where(f => f.Category == category.Value);

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(f => f.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(f => f.EntityId == entityId);

        var totalCount = await query.CountAsync(ct).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<StoredFile>> GetOrphanedFilesAsync(
        TimeSpan olderThan, int batchSize, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(olderThan);

        return await dbContext.StoredFiles
            .IgnoreQueryFilters()
            .Where(f => f.Status == FileStatus.PendingDeletion || f.Status == FileStatus.Orphaned)
            .Where(f => f.UpdatedAt < cutoff || (f.UpdatedAt == null && f.CreatedAt < cutoff))
            .OrderBy(f => f.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(StoredFile file, CancellationToken ct = default)
        => await dbContext.StoredFiles.AddAsync(file, ct).ConfigureAwait(false);

    public void Update(StoredFile file)
        => dbContext.StoredFiles.Update(file);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}
