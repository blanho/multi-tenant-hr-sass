using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Billing.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository(BillingDbContext dbContext) : ISubscriptionRepository
{
    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == id, ct).ConfigureAwait(false);

    public async Task<Subscription?> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await dbContext.Subscriptions
            .Where(s => s.TenantId == tenantId && s.Status != SubscriptionStatus.Cancelled && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task AddAsync(Subscription subscription, CancellationToken ct = default) =>
        await dbContext.Subscriptions.AddAsync(subscription, ct).ConfigureAwait(false);

    public void Update(Subscription subscription) => dbContext.Subscriptions.Update(subscription);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}
