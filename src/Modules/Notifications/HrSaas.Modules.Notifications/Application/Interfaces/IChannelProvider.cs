using HrSaas.Modules.Notifications.Domain.Enums;

namespace HrSaas.Modules.Notifications.Application.Interfaces;

public sealed record ChannelDeliveryResult(
    bool IsSuccess,
    string? ProviderResponse = null,
    string? ErrorMessage = null);

public interface IChannelProvider
{
    NotificationChannel Channel { get; }
    Task<ChannelDeliveryResult> SendAsync(ChannelMessage message, CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}

public sealed record ChannelMessage(
    Guid NotificationId,
    Guid TenantId,
    Guid UserId,
    string RecipientAddress,
    string Subject,
    string Body,
    NotificationPriority Priority,
    NotificationCategory Category,
    string? Metadata);

public interface IChannelProviderFactory
{
    IChannelProvider GetProvider(NotificationChannel channel);
    IReadOnlyList<NotificationChannel> GetAvailableChannels();
}

public interface ITemplateRenderer
{
    string Render(string template, IDictionary<string, string> variables);
}
