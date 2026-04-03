using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence.Repositories;

public sealed class LeaveRepository(LeaveDbContext dbContext) : ILeaveRepository
{
    public async Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<LeaveRequest>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default) =>
        await dbContext.LeaveRequests.Where(l => l.TenantId == tenantId && l.EmployeeId == employeeId && !l.IsDeleted).OrderByDescending(l => l.CreatedAt).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<LeaveRequest>> GetPendingAsync(Guid tenantId, CancellationToken ct = default) =>
        await dbContext.LeaveRequests.Where(l => l.TenantId == tenantId && l.Status == LeaveStatus.Pending && !l.IsDeleted).OrderBy(l => l.StartDate).ToListAsync(ct).ConfigureAwait(false);

    public async Task AddAsync(LeaveRequest request, CancellationToken ct = default) =>
        await dbContext.LeaveRequests.AddAsync(request, ct).ConfigureAwait(false);

    public void Update(LeaveRequest request) => dbContext.LeaveRequests.Update(request);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}
