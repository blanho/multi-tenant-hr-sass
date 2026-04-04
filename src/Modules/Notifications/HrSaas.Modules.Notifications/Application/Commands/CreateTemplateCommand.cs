using FluentValidation;
using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Notifications.Application.Commands;

public sealed record CreateTemplateCommand(
    Guid TenantId,
    string Name,
    string Slug,
    NotificationChannel Channel,
    NotificationCategory Category,
    string SubjectTemplate,
    string BodyTemplate,
    string? Description = null,
    string? SamplePayload = null) : ICommand<Guid>;

public sealed class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens only.");
        RuleFor(x => x.SubjectTemplate).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BodyTemplate).NotEmpty().MaximumLength(50000);
    }
}

public sealed class CreateTemplateCommandHandler(
    INotificationTemplateRepository repository)
    : IRequestHandler<CreateTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateTemplateCommand command,
        CancellationToken ct)
    {
        var existing = await repository.GetBySlugAsync(
            command.Slug, command.Channel, ct).ConfigureAwait(false);

        if (existing is not null)
            return Result<Guid>.Failure("A template with this slug and channel already exists.", "TEMPLATE_DUPLICATE");

        var template = NotificationTemplate.Create(
            command.TenantId,
            command.Name,
            command.Slug,
            command.Channel,
            command.Category,
            command.SubjectTemplate,
            command.BodyTemplate,
            command.Description,
            command.SamplePayload);

        await repository.AddAsync(template, ct).ConfigureAwait(false);
        await repository.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result<Guid>.Success(template.Id);
    }
}
