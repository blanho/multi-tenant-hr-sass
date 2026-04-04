using HrSaas.SharedKernel.CQRS;

namespace HrSaas.Modules.Storage.Application.Commands;

public sealed record DeleteFileCommand(
    Guid TenantId,
    Guid FileId) : ICommand;
