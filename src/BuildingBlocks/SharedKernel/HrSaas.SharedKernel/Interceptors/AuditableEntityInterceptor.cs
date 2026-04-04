using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HrSaas.SharedKernel.Interceptors;

public sealed class AuditableEntityInterceptor(IEntityChangeCollector changeCollector) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        OnBeforeSave(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        OnBeforeSave(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void OnBeforeSave(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        if (changeCollector is EntityChangeCollector collector)
        {
            collector.CaptureChanges(context);
        }

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Touch();
            }
        }
    }
}
