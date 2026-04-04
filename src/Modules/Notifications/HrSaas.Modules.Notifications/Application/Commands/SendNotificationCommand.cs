using FluentValidation;
using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Notifications.Application.Commands;

[Auditable(AuditAction.Send, AuditCategory.Notification, Severity = AuditSeverity.Low)]
public sealed record SendNotificationCommand(
    Guid TenantId,
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
    DateTime? ExpiresAt = null) : ICommand<Guid>;

public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(50000);
        RuleFor(x => x.RecipientAddress)
            .NotEmpty()
            .When(x => x.Channel is NotificationChannel.Email or NotificationChannel.Sms or NotificationChannel.Webhook);
        RuleFor(x => x.RecipientAddress)
            .EmailAddress()
            .When(x => x.Channel == NotificationChannel.Email && !string.IsNullOrEmpty(x.RecipientAddress));
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue);
    }
}

public sealed class SendNotificationCommandHandler(
    INotificationRepository notificationRepository,
    INotificationTemplateRepository templateRepository,
    IUserNotificationPreferenceRepository preferenceRepository,
    IChannelProviderFactory channelProviderFactory) : IRequestHandler<SendNotificationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        SendNotificationCommand command,
        CancellationToken ct)
    {
        var preference = await preferenceRepository.GetAsync(
            command.UserId, command.Channel, command.Category, ct).ConfigureAwait(false);

        if (preference is { IsEnabled: false })
            return Result<Guid>.Failure("User has disabled notifications for this channel and category.", "NOTIFICATIONS_DISABLED");

        if (preference is not null && preference.IsInQuietHours(DateTime.UtcNow) && command.Priority != NotificationPriority.Critical)
            return Result<Guid>.Failure("User is in quiet hours. Only critical notifications are delivered.", "QUIET_HOURS");

        var subject = command.Subject;
        var body = command.Body;
        Guid? templateId = null;

        if (!string.IsNullOrWhiteSpace(command.TemplateSlug) && command.TemplateVariables is not null)
        {
            var template = await templateRepository.GetBySlugAsync(
                command.TemplateSlug, command.Channel, ct).ConfigureAwait(false);

            if (template is not null && template.IsActive)
            {
                subject = template.RenderSubject(command.TemplateVariables);
                body = template.RenderBody(command.TemplateVariables);
                templateId = template.Id;
            }
        }

        var notification = Notification.Create(
            command.TenantId,
            command.UserId,
            command.Channel,
            command.Category,
            command.Priority,
            subject,
            body,
            command.RecipientAddress,
            templateId,
            command.CorrelationId,
            command.Metadata,
            command.ScheduledAt,
            command.ExpiresAt);

        await notificationRepository.AddAsync(notification, ct).ConfigureAwait(false);
        await notificationRepository.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result<Guid>.Success(notification.Id);
    }
}
