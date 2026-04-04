using HrSaas.Modules.Storage.Domain.Enums;
using HrSaas.SharedKernel.CQRS;

namespace HrSaas.Modules.Storage.Application.Commands;

public sealed record UploadFileCommand(
    Guid TenantId,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    FileCategory Category,
    Guid UploadedBy,
    string? EntityType = null,
    string? EntityId = null,
    string? Description = null) : ICommand<Guid>;
