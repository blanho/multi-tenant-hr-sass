using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence.Repositories;

public sealed class LeaveBalanceRepository(LeaveDbContext dbContext) : ILeaveBalanceRepository
{
    public async Task<LeaveBalance?> GetAsync(Guid tenantId, Guid employeeId, int year, CancellationToken ct = default) =>
        await dbContext.LeaveBalances
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.EmployeeId == employeeId && b.Year == year, ct)
            .ConfigureAwait(false);

    public async Task AddAsync(LeaveBalance balance, CancellationToken ct = default) =>
        await dbContext.LeaveBalances.AddAsync(balance, ct).ConfigureAwait(false);

    public void Update(LeaveBalance balance) => dbContext.LeaveBalances.Update(balance);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}
