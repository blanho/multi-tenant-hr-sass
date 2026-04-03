using HrSaas.Contracts.Notifications;
using HrSaas.Modules.Notifications.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Application.Consumers;

public sealed class SendEmailNotificationConsumer(
    INotificationService notificationService,
    ILogger<SendEmailNotificationConsumer> logger) : IConsumer<SendEmailNotificationCommand>
{
    public async Task Consume(ConsumeContext<SendEmailNotificationCommand> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Processing email notification to {ToEmail} for tenant {TenantId}. CorrelationId: {CorrelationId}",
            msg.ToEmail, msg.TenantId, msg.CorrelationId);

        await notificationService.SendEmailAsync(
            msg.ToEmail,
            msg.Subject,
            msg.BodyHtml,
            msg.BodyText,
            context.CancellationToken).ConfigureAwait(false);
    }
}
