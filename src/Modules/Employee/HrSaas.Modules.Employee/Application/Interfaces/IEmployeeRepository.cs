using HrSaas.Modules.Employee.Domain.Entities;
using HrSaas.SharedKernel.Entities;

namespace HrSaas.Modules.Employee.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Entities.Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Entities.Employee>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Entities.Employee>> GetByDepartmentAsync(string department, CancellationToken ct = default);

    Task AddAsync(Entities.Employee employee, CancellationToken ct = default);

    void Update(Entities.Employee employee);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
