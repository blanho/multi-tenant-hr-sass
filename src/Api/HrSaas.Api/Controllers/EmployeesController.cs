using Asp.Versioning;
using HrSaas.Api.Infrastructure.Authorization;
using HrSaas.Modules.Employee.Application.Commands;
using HrSaas.Modules.Employee.Application.DTOs;
using HrSaas.Modules.Employee.Application.Queries;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.SharedKernel.Pagination;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class EmployeesController(
    IMediator mediator,
    ITenantService tenantService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [HasPermission(Permission.Employees.View)]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetEmployeeByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet]
    [HasPermission(Permission.Employees.View)]
    [ProducesResponseType<PagedResult<EmployeeSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? department = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAllEmployeesQuery(page, pageSize, department), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission(Permission.Employees.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken ct)
    {
        var command = new CreateEmployeeCommand(
            tenantService.GetCurrentTenantId(),
            request.Name,
            request.Department,
            request.Position,
            request.Email);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permission.Employees.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken ct)
    {
        var command = new UpdateEmployeeCommand(
            tenantService.GetCurrentTenantId(),
            id,
            request.Name,
            request.Department,
            request.Position);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permission.Employees.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteEmployeeCommand(
            tenantService.GetCurrentTenantId(),
            id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }
}


public record CreateEmployeeRequest(
    string Name,
    string Department,
    string Position,
    string Email);

public record UpdateEmployeeRequest(
    string Name,
    string Department,
    string Position);
