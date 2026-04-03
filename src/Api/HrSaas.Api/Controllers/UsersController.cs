using Asp.Versioning;
using HrSaas.Api.Infrastructure.Authorization;
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
public sealed class UsersController(IMediator mediator, ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permission.Users.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetAllUsersQuery(tenantId), ct);
        return Ok(result.Value);
    }

    [HttpGet("{userId:guid}")]
    [HasPermission(Permission.Users.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid userId, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetUserByIdQuery(tenantId, userId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
