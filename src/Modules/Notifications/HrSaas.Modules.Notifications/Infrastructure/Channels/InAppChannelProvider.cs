using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Infrastructure.Channels;

public sealed class InAppChannelProvider(
    ILogger<InAppChannelProvider> logger) : IChannelProvider
{
    public NotificationChannel Channel => NotificationChannel.InApp;

    public Task<ChannelDeliveryResult> SendAsync(ChannelMessage message, CancellationToken ct = default)
    {
        logger.LogInformation(
            "In-app notification delivered for user {UserId}, notification {NotificationId}",
            message.UserId, message.NotificationId);

        return Task.FromResult(new ChannelDeliveryResult(true, "Stored as in-app notification"));
    }

    public Task<bool> IsAvailableAsync(CancellationToken ct = default) =>
        Task.FromResult(true);
}
