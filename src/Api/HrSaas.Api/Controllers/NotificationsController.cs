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
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class NotificationsController(
    IMediator mediator,
    ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<NotificationPagedResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] NotificationCategory? category = null,
        [FromQuery] NotificationChannel? channel = null,
        [FromQuery] bool? unreadOnly = null,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(
            new GetNotificationsQuery(userId, page, pageSize, category, channel, unreadOnly), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<NotificationDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetNotificationByIdQuery(id, userId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("unread-count")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetUnreadCountQuery(userId), ct);
        return Ok(new { count = result.Value });
    }

    [HttpPost]
    [HasPermission(Permission.Notifications.Send)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send(
        [FromBody] SendNotificationRequest request,
        CancellationToken ct)
    {
        var command = new SendNotificationCommand(
            tenantService.GetCurrentTenantId(),
            request.UserId,
            request.Channel,
            request.Category,
            request.Priority,
            request.Subject,
            request.Body,
            request.RecipientAddress,
            request.TemplateSlug,
            request.TemplateVariables,
            request.CorrelationId,
            request.Metadata,
            request.ScheduledAt,
            request.ExpiresAt);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    [HttpPost("bulk")]
    [HasPermission(Permission.Notifications.Send)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendBulk(
        [FromBody] SendBulkNotificationRequest request,
        CancellationToken ct)
    {
        var command = new SendBulkNotificationCommand(
            tenantService.GetCurrentTenantId(),
            request.UserIds,
            request.Channel,
            request.Category,
            request.Priority,
            request.Subject,
            request.Body,
            request.RecipientAddresses,
            request.CorrelationId,
            request.Metadata);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Created(string.Empty, new { ids = result.Value, count = result.Value!.Count })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new MarkNotificationReadCommand(id, userId), ct);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new MarkAllNotificationsReadCommand(userId), ct);
        return Ok(new { markedCount = result.Value });
    }

    [HttpPost("{id:guid}/retry")]
    [HasPermission(Permission.Notifications.Send)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Retry(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new RetryNotificationCommand(id), ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim is not null ? Guid.Parse(claim.Value) : Guid.Empty;
    }
}

public sealed record SendNotificationRequest(
    Guid UserId,
    NotificationChannel Channel,
    NotificationCategory Category,
    NotificationPriority Priority,
    string Subject,
    string Body,
    string? RecipientAddress = null,
    string? TemplateSlug = null,
    IDictionary<string, string>? TemplateVariables = null,
    string? CorrelationId = null,
    string? Metadata = null,
    DateTime? ScheduledAt = null,
    DateTime? ExpiresAt = null);

public sealed record SendBulkNotificationRequest(
    IReadOnlyList<Guid> UserIds,
    NotificationChannel Channel,
    NotificationCategory Category,
    NotificationPriority Priority,
    string Subject,
    string Body,
    IReadOnlyList<string>? RecipientAddresses = null,
    string? CorrelationId = null,
    string? Metadata = null);
