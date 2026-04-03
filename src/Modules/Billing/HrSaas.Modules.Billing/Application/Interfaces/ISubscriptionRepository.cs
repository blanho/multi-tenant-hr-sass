using HrSaas.Modules.Billing.Domain.Entities;

namespace HrSaas.Modules.Billing.Application.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Subscription?> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Subscription subscription, CancellationToken ct = default);
    void Update(Subscription subscription);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
