using Asp.Versioning;
using HrSaas.Api.Infrastructure.Authorization;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.Modules.Leave.Application.Commands;
using HrSaas.Modules.Leave.Application.Queries;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class LeaveController(IMediator mediator, ITenantService tenantService) : ControllerBase
{
    [HttpGet("employee/{employeeId:guid}")]
    [HasPermission(Permission.Leaves.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetLeavesByEmployeeQuery(tenantId, employeeId), ct);
        return Ok(result.Value);
    }

    [HttpGet("balance/{employeeId:guid}")]
    [HasPermission(Permission.Leaves.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance(Guid employeeId, [FromQuery] int? year, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetLeaveBalanceQuery(tenantId, employeeId, year), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("{leaveId:guid}")]
    [HasPermission(Permission.Leaves.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid leaveId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetLeaveByIdQuery(tenantId, leaveId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("pending")]
    [HasPermission(Permission.Leaves.Approve)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetPendingLeavesQuery(tenantId), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission(Permission.Leaves.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apply([FromBody] ApplyLeaveCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { leaveId = result.Value }, new { id = result.Value });
    }

    [HttpPost("{leaveId:guid}/approve")]
    [HasPermission(Permission.Leaves.Approve)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid leaveId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new ApproveLeaveCommand(tenantId, leaveId, userId), ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{leaveId:guid}/reject")]
    [HasPermission(Permission.Leaves.Reject)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(Guid leaveId, [FromBody] RejectRequest request, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new RejectLeaveCommand(tenantId, leaveId, userId, request.Note), ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("{leaveId:guid}")]
    [HasPermission(Permission.Leaves.Cancel)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid leaveId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var employeeId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new CancelLeaveCommand(tenantId, leaveId, employeeId), ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error);
        }

        return NoContent();
    }
}

public sealed record RejectRequest(string Note);
