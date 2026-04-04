using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HrSaas.Modules.Notifications.Infrastructure.Channels;

public sealed class SlackChannelProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<SlackOptions> options,
    ILogger<SlackChannelProvider> logger) : IChannelProvider
{
    private readonly SlackOptions _slack = options.Value;

    public NotificationChannel Channel => NotificationChannel.Slack;

    public async Task<ChannelDeliveryResult> SendAsync(ChannelMessage message, CancellationToken ct = default)
    {
        var webhookUrl = !string.IsNullOrWhiteSpace(message.RecipientAddress)
            ? message.RecipientAddress
            : _slack.DefaultWebhookUrl;

        if (string.IsNullOrWhiteSpace(webhookUrl))
            return new ChannelDeliveryResult(false, ErrorMessage: "Slack webhook URL not configured");

        try
        {
            using var client = httpClientFactory.CreateClient("SlackNotification");

            var priorityEmoji = message.Priority switch
            {
                NotificationPriority.Critical => ":rotating_light:",
                NotificationPriority.High => ":warning:",
                NotificationPriority.Normal => ":bell:",
                _ => ":information_source:"
            };

            var slackPayload = new
            {
                blocks = new object[]
                {
                    new
                    {
                        type = "header",
                        text = new { type = "plain_text", text = $"{priorityEmoji} {message.Subject}" }
                    },
                    new
                    {
                        type = "section",
                        text = new { type = "mrkdwn", text = message.Body }
                    },
                    new
                    {
                        type = "context",
                        elements = new[]
                        {
                            new { type = "mrkdwn", text = $"Category: *{message.Category}* | Priority: *{message.Priority}*" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(slackPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(webhookUrl, content, ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "Slack notification sent for {NotificationId}",
                    message.NotificationId);
                return new ChannelDeliveryResult(true, "Slack message sent");
            }

            var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return new ChannelDeliveryResult(false, ErrorMessage: $"Slack API error: {responseBody}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Slack notification failed for {NotificationId}", message.NotificationId);
            return new ChannelDeliveryResult(false, ErrorMessage: ex.Message);
        }
    }

    public Task<bool> IsAvailableAsync(CancellationToken ct = default) =>
        Task.FromResult(!string.IsNullOrWhiteSpace(_slack.DefaultWebhookUrl));
}

public sealed class SlackOptions
{
    public const string SectionName = "Slack";
    public string? DefaultWebhookUrl { get; set; }
}
