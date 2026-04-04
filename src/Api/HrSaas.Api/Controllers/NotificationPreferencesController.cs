using Asp.Versioning;
using HrSaas.Api.Infrastructure.Authorization;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.Modules.Notifications.Application.Commands;
using HrSaas.Modules.Notifications.Application.DTOs;
using HrSaas.Modules.Notifications.Application.Queries;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/notifications/preferences")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class NotificationPreferencesController(
    IMediator mediator,
    ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserPreferenceDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetUserPreferencesQuery(userId), ct);
        return Ok(result.Value);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePreference(
        [FromBody] UpdatePreferenceRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateUserPreferencesCommand(
            tenantService.GetCurrentTenantId(),
            userId,
            request.Channel,
            request.Category,
            request.IsEnabled,
            request.DigestFrequency,
            request.QuietHoursStart,
            request.QuietHoursEnd,
            request.TimeZone);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim is not null ? Guid.Parse(claim.Value) : Guid.Empty;
    }
}

public sealed record UpdatePreferenceRequest(
    NotificationChannel Channel,
    NotificationCategory Category,
    bool IsEnabled,
    DigestFrequency DigestFrequency = DigestFrequency.Immediate,
    TimeOnly? QuietHoursStart = null,
    TimeOnly? QuietHoursEnd = null,
    string? TimeZone = null);
