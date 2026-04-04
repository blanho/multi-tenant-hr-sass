using HrSaas.Modules.Storage.Domain.Repositories;
using HrSaas.SharedKernel.Jobs;
using HrSaas.SharedKernel.Storage;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Storage.Jobs;

public sealed class OrphanedFileCleanupJob(
    IStoredFileRepository repository,
    IStorageProvider storageProvider,
    ILogger<OrphanedFileCleanupJob> logger) : IRecurringJob
{
    private const int BatchSize = 100;
    private static readonly TimeSpan OrphanedThreshold = TimeSpan.FromDays(30);

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var orphanedFiles = await repository.GetOrphanedFilesAsync(OrphanedThreshold, BatchSize, ct)
            .ConfigureAwait(false);

        if (orphanedFiles.Count == 0)
            return;

        var deleted = 0;
        var failed = 0;

        foreach (var file in orphanedFiles)
        {
            try
            {
                await storageProvider.DeleteAsync(file.TenantId, file.BlobName, ct).ConfigureAwait(false);
                file.Delete();
                deleted++;
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogError(ex,
                    "Failed to delete orphaned blob {BlobName} for file {FileId}",
                    file.BlobName, file.Id);
            }
        }

        if (deleted > 0)
        {
            await repository.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        logger.LogInformation(
            "Orphaned file cleanup completed: {Deleted} deleted, {Failed} failed out of {Total}",
            deleted, failed, orphanedFiles.Count);
    }
}
