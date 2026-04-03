namespace HrSaas.Modules.Employee.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Domain.Entities.Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Domain.Entities.Employee>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Domain.Entities.Employee>> GetByDepartmentAsync(
        string department,
        CancellationToken ct = default);

    Task AddAsync(Domain.Entities.Employee employee, CancellationToken ct = default);

    void Update(Domain.Entities.Employee employee);

    void Delete(Domain.Entities.Employee employee);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
