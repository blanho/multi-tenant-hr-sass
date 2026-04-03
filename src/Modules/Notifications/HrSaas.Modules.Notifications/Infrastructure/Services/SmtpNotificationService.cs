using HrSaas.Modules.Notifications.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HrSaas.Modules.Notifications.Infrastructure.Services;

public sealed class SmtpNotificationService(
    IOptions<SmtpOptions> options,
    ILogger<SmtpNotificationService> logger) : INotificationService
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string bodyHtml,
        string? bodyText = null,
        CancellationToken ct = default)
    {
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromDisplayName),
            Subject = subject,
            Body = bodyHtml,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        if (!string.IsNullOrWhiteSpace(bodyText))
        {
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(bodyText, null, "text/plain"));
        }

        await client.SendMailAsync(message, ct).ConfigureAwait(false);

        logger.LogInformation("Email sent to {ToEmail} with subject '{Subject}'.", toEmail, subject);
    }
}

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@hrsampl.io";
    public string FromDisplayName { get; set; } = "HrSaas Platform";
}
