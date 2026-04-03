using HrSaas.Modules.Leave.Domain.Entities;

namespace HrSaas.Modules.Leave.Application.Interfaces;

public interface ILeaveRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetPendingAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(LeaveRequest request, CancellationToken ct = default);
    void Update(LeaveRequest request);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
