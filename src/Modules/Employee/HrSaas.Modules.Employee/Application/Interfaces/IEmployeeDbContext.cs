using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Employee.Application.Interfaces;

public interface IEmployeeDbContext
{
    DbSet<Domain.Entities.Employee> Employees { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
