using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.SharedKernel.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Application.Commands;

public sealed record RetryNotificationCommand(Guid NotificationId) : ICommand<Guid>;

public sealed class RetryNotificationCommandHandler(
    INotificationRepository repository,
    IChannelProviderFactory channelProviderFactory,
    ILogger<RetryNotificationCommandHandler> logger)
    : IRequestHandler<RetryNotificationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        RetryNotificationCommand command,
        CancellationToken ct)
    {
        var notification = await repository.GetByIdWithAttemptsAsync(
            command.NotificationId, ct).ConfigureAwait(false);

        if (notification is null)
            return Result<Guid>.Failure("Notification not found.", "NOTIFICATION_NOT_FOUND");

        if (!notification.CanRetry)
            return Result<Guid>.Failure("Notification cannot be retried.", "RETRY_NOT_ALLOWED");

        var provider = channelProviderFactory.GetProvider(notification.Channel);
        notification.MarkAsSending();

        var result = await provider.SendAsync(new ChannelMessage(
            notification.Id,
            notification.TenantId,
            notification.UserId,
            notification.RecipientAddress ?? string.Empty,
            notification.Subject,
            notification.Body,
            notification.Priority,
            notification.Category,
            notification.Metadata), ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            notification.RecordDeliveryAttempt(DeliveryStatus.Succeeded, result.ProviderResponse);
            notification.MarkAsDelivered(result.ProviderResponse);
        }
        else
        {
            notification.RecordDeliveryAttempt(DeliveryStatus.Failed, errorMessage: result.ErrorMessage);
            notification.MarkAsFailed(result.ErrorMessage ?? "Unknown delivery error");
        }

        repository.Update(notification);
        await repository.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation(
            "Retry attempt for notification {NotificationId} on channel {Channel}: {Status}",
            notification.Id, notification.Channel, result.IsSuccess ? "Succeeded" : "Failed");

        return Result<Guid>.Success(notification.Id);
    }
}
