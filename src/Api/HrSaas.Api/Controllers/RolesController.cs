using Asp.Versioning;
using HrSaas.Api.Infrastructure.Authorization;
using HrSaas.Modules.Identity.Application.Commands;
using HrSaas.Modules.Identity.Application.Queries;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.TenantSdk;
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
public sealed class RolesController(IMediator mediator, ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permission.Roles.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetRolesQuery(tenantId), ct);
        return Ok(result.Value);
    }

    [HttpGet("{roleId:guid}")]
    [HasPermission(Permission.Roles.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid roleId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetRoleByIdQuery(tenantId, roleId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("permissions")]
    [HasPermission(Permission.Roles.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailablePermissions(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAvailablePermissionsQuery(), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission(Permission.Roles.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(
            new CreateRoleCommand(tenantId, request.Name, request.Permissions), ct);

        if (!result.IsSuccess)
            return result.ErrorCode == "ROLE_EXISTS" ? Conflict(result.Error) : BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { roleId = result.Value }, new { id = result.Value });
    }

    [HttpPut("{roleId:guid}/permissions")]
    [HasPermission(Permission.Roles.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePermissions(
        Guid roleId,
        [FromBody] UpdatePermissionsRequest request,
        CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(
            new UpdateRolePermissionsCommand(tenantId, roleId, request.Permissions), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpPost("assign")]
    [HasPermission(Permission.Users.AssignRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(
            new AssignRoleCommand(tenantId, request.UserId, request.RoleId), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpDelete("{roleId:guid}")]
    [HasPermission(Permission.Roles.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new DeleteRoleCommand(tenantId, roleId), ct);

        if (!result.IsSuccess)
            return result.ErrorCode == "SYSTEM_ROLE" ? BadRequest(result.Error) : NotFound(result.Error);

        return NoContent();
    }
}

public sealed record CreateRoleRequest(string Name, IReadOnlyList<string> Permissions);
public sealed record UpdatePermissionsRequest(IReadOnlyList<string> Permissions);
public sealed record AssignRoleRequest(Guid UserId, Guid RoleId);
