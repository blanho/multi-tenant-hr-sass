using Asp.Versioning;
using HrSaas.Api.Infrastructure.Authorization;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.Modules.Tenant.Application.Commands;
using HrSaas.Modules.Tenant.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{tenantId:guid}")]
    [HasPermission(Permission.Tenants.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(tenantId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet]
    [HasPermission(Permission.Tenants.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllTenantsQuery(), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission(Permission.Tenants.Create)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "SLUG_TAKEN" ? Conflict(result.Error) : BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { tenantId = result.Value }, new { id = result.Value });
    }

    [HttpPut("{tenantId:guid}")]
    [HasPermission(Permission.Tenants.Create)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid tenantId, [FromBody] UpdateTenantRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateTenantCommand(tenantId, request.Name, request.ContactEmail), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpPost("{tenantId:guid}/suspend")]
    [HasPermission(Permission.Tenants.Suspend)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid tenantId, [FromBody] SuspendRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuspendTenantCommand(tenantId, request.Reason), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpPost("{tenantId:guid}/reinstate")]
    [HasPermission(Permission.Tenants.Reinstate)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reinstate(Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new ReinstateCommand(tenantId), ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{tenantId:guid}/upgrade-plan")]
    [HasPermission(Permission.Tenants.UpgradePlan)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpgradePlan(Guid tenantId, [FromBody] UpgradePlanRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpgradePlanCommand(tenantId, request.NewPlan), ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error);
        }

        return NoContent();
    }
}

public sealed record SuspendRequest(string Reason);
public sealed record UpgradePlanRequest(HrSaas.Modules.Tenant.Domain.Entities.PlanType NewPlan);
public sealed record UpdateTenantRequest(string Name, string ContactEmail);
