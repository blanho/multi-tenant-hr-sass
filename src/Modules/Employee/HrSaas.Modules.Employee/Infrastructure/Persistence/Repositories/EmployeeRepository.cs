using HrSaas.Modules.Employee.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Employee.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(EmployeeDbContext dbContext) : IEmployeeRepository
{
    public async Task<Domain.Entities.Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Domain.Entities.Employee>> GetAllAsync(CancellationToken ct = default)
        => await dbContext.Employees
            .OrderBy(e => e.Name)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Domain.Entities.Employee>> GetByDepartmentAsync(
        string department, CancellationToken ct = default)
        => await dbContext.Employees
            .Where(e => e.Department.Name == department)
            .OrderBy(e => e.Name)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task AddAsync(Domain.Entities.Employee employee, CancellationToken ct = default)
        => await dbContext.Employees.AddAsync(employee, ct).ConfigureAwait(false);

    public void Update(Domain.Entities.Employee employee)
        => dbContext.Employees.Update(employee);

    public void Delete(Domain.Entities.Employee employee)
    {
        dbContext.Employees.Remove(employee);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
