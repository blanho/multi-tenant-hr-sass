using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Notifications.Application.Commands;

[Auditable(AuditAction.Send, AuditCategory.Notification, Severity = AuditSeverity.Medium)]
public sealed record SendBulkNotificationCommand(
    Guid TenantId,
    IReadOnlyList<Guid> UserIds,
    NotificationChannel Channel,
    NotificationCategory Category,
    NotificationPriority Priority,
    string Subject,
    string Body,
    IReadOnlyList<string>? RecipientAddresses = null,
    string? CorrelationId = null,
    string? Metadata = null) : ICommand<IReadOnlyList<Guid>>;

public sealed class SendBulkNotificationCommandHandler(
    INotificationRepository notificationRepository)
    : IRequestHandler<SendBulkNotificationCommand, Result<IReadOnlyList<Guid>>>
{
    public async Task<Result<IReadOnlyList<Guid>>> Handle(
        SendBulkNotificationCommand command,
        CancellationToken ct)
    {
        var notifications = new List<Notification>();
        var groupId = Guid.NewGuid();

        for (var i = 0; i < command.UserIds.Count; i++)
        {
            var recipientAddress = command.RecipientAddresses is not null && i < command.RecipientAddresses.Count
                ? command.RecipientAddresses[i]
                : null;

            var notification = Notification.Create(
                command.TenantId,
                command.UserIds[i],
                command.Channel,
                command.Category,
                command.Priority,
                command.Subject,
                command.Body,
                recipientAddress,
                correlationId: command.CorrelationId,
                metadata: command.Metadata,
                groupId: groupId);

            notifications.Add(notification);
        }

        await notificationRepository.AddRangeAsync(notifications, ct).ConfigureAwait(false);
        await notificationRepository.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result<IReadOnlyList<Guid>>.Success(
            notifications.Select(n => n.Id).ToList().AsReadOnly());
    }
}
