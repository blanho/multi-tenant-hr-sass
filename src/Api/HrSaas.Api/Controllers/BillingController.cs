using Asp.Versioning;
using HrSaas.Api.Infrastructure.Authorization;
using HrSaas.Modules.Billing.Application.Commands;
using HrSaas.Modules.Billing.Application.Queries;
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
public sealed class BillingController(IMediator mediator, ITenantService tenantService) : ControllerBase
{
    [HttpGet("subscription")]
    [HasPermission(Permission.Billing.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscription(CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new GetSubscriptionByTenantQuery(tenantId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("subscription/create-free")]
    [HasPermission(Permission.Billing.View)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateFree(CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new CreateFreeSubscriptionCommand(tenantId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetSubscription), new { id = result.Value })
            : Conflict(result.Error);
    }

    [HttpPost("subscription/{subscriptionId:guid}/activate")]
    [HasPermission(Permission.Billing.View)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(
        Guid subscriptionId,
        [FromBody] ActivateRequest request,
        CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(
            new ActivateSubscriptionCommand(tenantId, subscriptionId, request.Price, request.Cycle, request.ExternalId), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpPost("subscription/cancel")]
    [HasPermission(Permission.Billing.Cancel)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel([FromBody] CancelRequest request, CancellationToken ct)
    {
        var tenantId = tenantService.GetCurrentTenantId();
        var result = await mediator.Send(new CancelSubscriptionCommand(tenantId, request.SubscriptionId, request.Reason), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}

public sealed record CancelRequest(Guid SubscriptionId, string Reason);
public sealed record ActivateRequest(decimal Price, HrSaas.Modules.Billing.Domain.Entities.BillingCycle Cycle, string? ExternalId);
