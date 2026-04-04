using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Notifications.Application.Commands;

public sealed record MarkNotificationReadCommand(Guid NotificationId, Guid UserId) : ICommand;

public sealed class MarkNotificationReadCommandHandler(
    INotificationRepository repository)
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    public async Task<Result> Handle(
        MarkNotificationReadCommand command,
        CancellationToken ct)
    {
        var notification = await repository.GetByIdAsync(command.NotificationId, ct).ConfigureAwait(false);

        if (notification is null)
            return Result.Failure("Notification not found.", "NOTIFICATION_NOT_FOUND");

        if (notification.UserId != command.UserId)
            return Result.Failure("Access denied.", "ACCESS_DENIED");

        notification.MarkAsRead();
        repository.Update(notification);
        await repository.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Success();
    }
}

public sealed record MarkAllNotificationsReadCommand(Guid UserId) : ICommand<int>;

public sealed class MarkAllNotificationsReadCommandHandler(
    INotificationRepository repository)
    : IRequestHandler<MarkAllNotificationsReadCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        MarkAllNotificationsReadCommand command,
        CancellationToken ct)
    {
        var unread = await repository.GetUnreadByUserIdAsync(command.UserId, ct).ConfigureAwait(false);

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
            repository.Update(notification);
        }

        var count = await repository.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result<int>.Success(unread.Count);
    }
}
