using FluentValidation;
using HrSaas.Modules.Storage.Application.Commands;
using Microsoft.Extensions.Options;
using HrSaas.SharedKernel.Storage;

namespace HrSaas.Modules.Storage.Application.Validators;

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator(IOptions<StorageProviderOptions> options)
    {
        var config = options.Value;

        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => config.AllowedContentTypes.Contains(ct))
            .WithMessage(x =>
                $"Content type '{x.ContentType}' is not allowed. Allowed types: {string.Join(", ", config.AllowedContentTypes)}");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(config.MaxFileSizeBytes)
            .WithMessage(x =>
                $"File size {x.SizeBytes} bytes exceeds the maximum allowed size of {config.MaxFileSizeBytes} bytes.");

        RuleFor(x => x.Content)
            .NotNull();

        RuleFor(x => x.UploadedBy)
            .NotEmpty();
    }
}

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.FileId).NotEmpty();
    }
}
