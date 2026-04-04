using Asp.Versioning;
using HrSaas.Api.Infrastructure.Authorization;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.Modules.Storage.Application.Commands;
using HrSaas.Modules.Storage.Application.DTOs;
using HrSaas.Modules.Storage.Application.Queries;
using HrSaas.Modules.Storage.Domain.Enums;
using HrSaas.SharedKernel.Pagination;
using HrSaas.SharedKernel.Storage;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class FilesController(
    IMediator mediator,
    ITenantService tenantService,
    IStorageProvider storageProvider,
    IOptions<StorageProviderOptions> storageOptions) : ControllerBase
{
    [HttpPost]
    [HasPermission(Permission.Files.Upload)]
    [ProducesResponseType<FileUploadResultDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] FileCategory category = FileCategory.General,
        [FromForm] string? entityType = null,
        [FromForm] string? entityId = null,
        [FromForm] string? description = null,
        CancellationToken ct = default)
    {
        if (file.Length == 0)
            return BadRequest(new { error = "File is empty." });

        var config = storageOptions.Value;

        if (file.Length > config.MaxFileSizeBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                new { error = $"File size exceeds the maximum allowed size of {config.MaxFileSizeBytes} bytes." });

        if (!config.AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(new { error = $"Content type '{file.ContentType}' is not allowed." });

        var userId = GetUserId();
        await using var stream = file.OpenReadStream();

        var command = new UploadFileCommand(
            tenantService.GetCurrentTenantId(),
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            category,
            userId,
            entityType,
            entityId,
            description);

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var uploadResult = new FileUploadResultDto(
            result.Value,
            file.FileName,
            file.Length,
            file.ContentType);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, uploadResult);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permission.Files.View)]
    [ProducesResponseType<StoredFileDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetStoredFileByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet]
    [HasPermission(Permission.Files.View)]
    [ProducesResponseType<PagedResult<StoredFileDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] FileCategory? category = null,
        [FromQuery] FileStatus? status = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new ListStoredFilesQuery(page, pageSize, category, status, entityType, entityId), ct);
        return Ok(result.Value);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    [HasPermission(Permission.Files.View)]
    [ProducesResponseType<IReadOnlyList<StoredFileDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        string entityId,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetFilesByEntityQuery(entityType, entityId), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/download")]
    [HasPermission(Permission.Files.Download)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var fileResult = await mediator.Send(new GetStoredFileByIdQuery(id), ct);
        if (!fileResult.IsSuccess)
            return NotFound(new { error = fileResult.Error });

        var fileDetail = fileResult.Value!;
        var tenantId = tenantService.GetCurrentTenantId();

        var stream = await storageProvider.DownloadAsync(tenantId, fileDetail.BlobName, ct);
        if (stream is null)
            return NotFound(new { error = "File blob not found in storage." });

        return File(stream, fileDetail.ContentType, fileDetail.OriginalFileName);
    }

    [HttpGet("{id:guid}/presigned-url")]
    [HasPermission(Permission.Files.Download)]
    [ProducesResponseType<PresignedUrlDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPresignedUrl(
        Guid id,
        [FromQuery] int expiryMinutes = 60,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GeneratePresignedUrlQuery(id, expiryMinutes), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permission.Files.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteFileCommand(tenantService.GetCurrentTenantId(), id);
        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var userId) ? userId : Guid.Empty;
    }
}
