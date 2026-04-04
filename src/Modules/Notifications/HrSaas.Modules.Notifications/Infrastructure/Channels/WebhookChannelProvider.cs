using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace HrSaas.Modules.Notifications.Infrastructure.Channels;

public sealed class WebhookChannelProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<WebhookChannelProvider> logger) : IChannelProvider
{
    public NotificationChannel Channel => NotificationChannel.Webhook;

    public async Task<ChannelDeliveryResult> SendAsync(ChannelMessage message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message.RecipientAddress))
            return new ChannelDeliveryResult(false, ErrorMessage: "Webhook URL is required");

        try
        {
            using var client = httpClientFactory.CreateClient("NotificationWebhook");

            var payload = new
            {
                notificationId = message.NotificationId,
                tenantId = message.TenantId,
                userId = message.UserId,
                subject = message.Subject,
                body = message.Body,
                priority = message.Priority.ToString(),
                category = message.Category.ToString(),
                timestamp = DateTime.UtcNow
            };

            var response = await client.PostAsJsonAsync(
                message.RecipientAddress, payload, ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "Webhook delivered to {Url} for notification {NotificationId}",
                    message.RecipientAddress, message.NotificationId);

                return new ChannelDeliveryResult(true, $"HTTP {(int)response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            logger.LogWarning(
                "Webhook delivery failed to {Url}: HTTP {StatusCode}",
                message.RecipientAddress, (int)response.StatusCode);

            return new ChannelDeliveryResult(false, ErrorMessage: $"HTTP {(int)response.StatusCode}: {responseBody}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Webhook delivery failed to {Url}", message.RecipientAddress);
            return new ChannelDeliveryResult(false, ErrorMessage: ex.Message);
        }
    }

    public Task<bool> IsAvailableAsync(CancellationToken ct = default) =>
        Task.FromResult(true);
}
