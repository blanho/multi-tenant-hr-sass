namespace HrSaas.Modules.Notifications.Application.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(
        string toEmail,
        string subject,
        string bodyHtml,
        string? bodyText = null,
        CancellationToken ct = default);
}
