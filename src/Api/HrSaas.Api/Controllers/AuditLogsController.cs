using Asp.Versioning;
using HrSaas.Modules.Audit.Application.DTOs;
using HrSaas.Modules.Audit.Application.Queries;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("api")]
public sealed class AuditLogsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<AuditLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] AuditCategory? category = null,
        [FromQuery] AuditAction? action = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] bool? isSuccess = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAuditLogsQuery(page, pageSize, category, action, userId,
                entityType, entityId, from, to, isSuccess), ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AuditLogDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAuditLogByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType<PagedResult<AuditLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        string entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAuditLogsForEntityQuery(entityType, entityId, page, pageSize), ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
