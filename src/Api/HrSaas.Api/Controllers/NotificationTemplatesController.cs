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

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/notifications/templates")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("api")]
public sealed class NotificationTemplatesController(
    IMediator mediator,
    ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NotificationTemplateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] NotificationCategory? category = null,
        [FromQuery] bool? activeOnly = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetNotificationTemplatesQuery(category, activeOnly), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission(Permission.Notifications.Manage)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateTemplateRequest request,
        CancellationToken ct)
    {
        var command = new CreateTemplateCommand(
            tenantService.GetCurrentTenantId(),
            request.Name,
            request.Slug,
            request.Channel,
            request.Category,
            request.SubjectTemplate,
            request.BodyTemplate,
            request.Description,
            request.SamplePayload);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Created(string.Empty, new { id = result.Value })
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }
}

public sealed record CreateTemplateRequest(
    string Name,
    string Slug,
    NotificationChannel Channel,
    NotificationCategory Category,
    string SubjectTemplate,
    string BodyTemplate,
    string? Description = null,
    string? SamplePayload = null);
