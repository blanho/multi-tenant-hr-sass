using HrSaas.Modules.Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Employee.Infrastructure.Persistence;

public interface IEmployeeDbContext
{
    DbSet<Domain.Entities.Employee> Employees { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
