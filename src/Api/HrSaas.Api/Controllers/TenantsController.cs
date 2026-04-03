using Asp.Versioning;
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
[Authorize(Roles = "Admin")]
[EnableRateLimiting("api")]
public sealed class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{tenantId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(tenantId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllTenantsQuery(), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
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

    [HttpPost("{tenantId:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid tenantId, [FromBody] SuspendRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuspendTenantCommand(tenantId, request.Reason), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}

public sealed record SuspendRequest(string Reason);
