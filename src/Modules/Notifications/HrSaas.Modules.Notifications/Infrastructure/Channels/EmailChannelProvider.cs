using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HrSaas.Modules.Notifications.Infrastructure.Channels;

public sealed class EmailChannelProvider(
    IOptions<SmtpOptions> options,
    ILogger<EmailChannelProvider> logger) : IChannelProvider
{
    private readonly SmtpOptions _smtp = options.Value;

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<ChannelDeliveryResult> SendAsync(ChannelMessage message, CancellationToken ct = default)
    {
        try
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                EnableSsl = _smtp.EnableSsl,
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                Timeout = _smtp.TimeoutMs
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtp.FromAddress, _smtp.FromDisplayName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(message.RecipientAddress);

            if (message.Priority == NotificationPriority.Critical)
                mailMessage.Priority = MailPriority.High;

            await client.SendMailAsync(mailMessage, ct).ConfigureAwait(false);

            logger.LogInformation(
                "Email sent to {RecipientAddress} for notification {NotificationId}",
                message.RecipientAddress, message.NotificationId);

            return new ChannelDeliveryResult(true, "Email sent successfully via SMTP");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to send email to {RecipientAddress} for notification {NotificationId}",
                message.RecipientAddress, message.NotificationId);

            return new ChannelDeliveryResult(false, ErrorMessage: ex.Message);
        }
    }

    public Task<bool> IsAvailableAsync(CancellationToken ct = default) =>
        Task.FromResult(!string.IsNullOrEmpty(_smtp.Host));
}

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@hrsaas.io";
    public string FromDisplayName { get; set; } = "HrSaas Platform";
    public int TimeoutMs { get; set; } = 30000;
}
