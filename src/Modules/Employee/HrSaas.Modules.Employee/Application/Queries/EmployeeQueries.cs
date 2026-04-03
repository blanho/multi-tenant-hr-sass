using HrSaas.Modules.Employee.Application.DTOs;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Employee.Application.Queries;


public record GetEmployeeByIdQuery(Guid EmployeeId) : IQuery<EmployeeDto>;

public sealed class GetEmployeeByIdQueryHandler(IEmployeeDbContext dbContext)
    : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(
        GetEmployeeByIdQuery query,
        CancellationToken ct)
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
            .FirstOrDefaultAsync(ct)
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
        CancellationToken ct)
    {
        var baseQuery = dbContext.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Department))
            baseQuery = baseQuery.Where(e => e.Department.Name == query.Department);

        var totalCount = await baseQuery.CountAsync(ct).ConfigureAwait(false);

        var items = await baseQuery
            .OrderBy(e => e.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EmployeeSummaryDto(
                e.Id,
                e.Name,
                e.Department.Name,
                e.Position.Title))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var result = new PagedResult<EmployeeSummaryDto>(
            items.AsReadOnly(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result<PagedResult<EmployeeSummaryDto>>.Success(result);
    }
}


public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
