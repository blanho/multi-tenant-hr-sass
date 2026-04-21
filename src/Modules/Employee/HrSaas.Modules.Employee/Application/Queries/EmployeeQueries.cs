using HrSaas.Modules.Employee.Application.DTOs;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using HrSaas.SharedKernel.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Employee.Application.Queries;


public record GetEmployeeByIdQuery(Guid EmployeeId) : IQuery<EmployeeDto>;

public sealed class GetEmployeeByIdQueryHandler(IEmployeeDbContext dbContext)
    : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(
        GetEmployeeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .AsNoTracking()
            .Where(e => e.Id == query.EmployeeId)
            .Select(e => new EmployeeDto(
                e.Id,
                e.TenantId,
                e.Name,
                e.Department.Name,
                e.Position.Title,
                e.Email,
                e.CreatedAt,
                e.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return employee is null
            ? Result<EmployeeDto>.Failure("Employee not found.", "EMPLOYEE_NOT_FOUND")
            : Result<EmployeeDto>.Success(employee);
    }
}


public record GetAllEmployeesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Department = null) : IQuery<PagedResult<EmployeeSummaryDto>>;

public sealed class GetAllEmployeesQueryHandler(IEmployeeDbContext dbContext)
    : IRequestHandler<GetAllEmployeesQuery, Result<PagedResult<EmployeeSummaryDto>>>
{
    public async Task<Result<PagedResult<EmployeeSummaryDto>>> Handle(
        GetAllEmployeesQuery query,
        CancellationToken cancellationToken)
    {
        var baseQuery = dbContext.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            baseQuery = baseQuery.Where(e => e.Department.Name == query.Department);
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await baseQuery
            .OrderBy(e => e.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EmployeeSummaryDto(
                e.Id,
                e.Name,
                e.Department.Name,
                e.Position.Title))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = new PagedResult<EmployeeSummaryDto>(
            items.AsReadOnly(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result<PagedResult<EmployeeSummaryDto>>.Success(result);
    }
}

