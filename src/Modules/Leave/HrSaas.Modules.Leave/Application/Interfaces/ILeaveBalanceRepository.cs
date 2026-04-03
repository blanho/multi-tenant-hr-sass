using HrSaas.Modules.Leave.Domain.Entities;

namespace HrSaas.Modules.Leave.Application.Interfaces;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetAsync(Guid tenantId, Guid employeeId, int year, CancellationToken ct = default);
    Task AddAsync(LeaveBalance balance, CancellationToken ct = default);
    void Update(LeaveBalance balance);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
