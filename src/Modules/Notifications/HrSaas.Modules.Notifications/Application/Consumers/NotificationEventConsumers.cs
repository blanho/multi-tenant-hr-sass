using HrSaas.Contracts.Employee;
using HrSaas.Contracts.Leave;
using HrSaas.Contracts.Billing;
using HrSaas.Contracts.Identity;
using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Application.Consumers;

public sealed class EmployeeCreatedNotificationConsumer(
    INotificationRepository repository,
    ILogger<EmployeeCreatedNotificationConsumer> logger) : IConsumer<EmployeeCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EmployeeCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "Creating onboarding notification for employee {EmployeeId} in tenant {TenantId}",
            msg.EmployeeId, msg.TenantId);

        var notification = Notification.Create(
            msg.TenantId,
            msg.EmployeeId,
            NotificationChannel.InApp,
            NotificationCategory.Onboarding,
            NotificationPriority.Normal,
            "Welcome to the team!",
            $"Your account has been created. Welcome aboard, {msg.Name}! Please complete your onboarding tasks.",
            correlationId: msg.Id.ToString());

        await repository.AddAsync(notification, context.CancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}

public sealed class LeaveAppliedNotificationConsumer(
    INotificationRepository repository,
    ILogger<LeaveAppliedNotificationConsumer> logger) : IConsumer<LeaveAppliedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LeaveAppliedIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "Creating leave request notification for tenant {TenantId}",
            msg.TenantId);

        var notification = Notification.Create(
            msg.TenantId,
            msg.EmployeeId,
            NotificationChannel.InApp,
            NotificationCategory.Leave,
            NotificationPriority.Normal,
            "Leave Request Submitted",
            $"Your {msg.LeaveType} leave request from {msg.StartDate:yyyy-MM-dd} to {msg.EndDate:yyyy-MM-dd} ({msg.DurationDays} days) has been submitted for approval.",
            correlationId: msg.Id.ToString());

        await repository.AddAsync(notification, context.CancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}

public sealed class LeaveApprovedNotificationConsumer(
    INotificationRepository repository,
    ILogger<LeaveApprovedNotificationConsumer> logger) : IConsumer<LeaveApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LeaveApprovedIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "Creating leave approved notification for employee {EmployeeId} in tenant {TenantId}",
            msg.EmployeeId, msg.TenantId);

        var notification = Notification.Create(
            msg.TenantId,
            msg.EmployeeId,
            NotificationChannel.InApp,
            NotificationCategory.Leave,
            NotificationPriority.Normal,
            "Leave Request Approved",
            "Your leave request has been approved.",
            correlationId: msg.Id.ToString());

        await repository.AddAsync(notification, context.CancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}

public sealed class LeaveRejectedNotificationConsumer(
    INotificationRepository repository,
    ILogger<LeaveRejectedNotificationConsumer> logger) : IConsumer<LeaveRejectedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LeaveRejectedIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "Creating leave rejected notification for employee {EmployeeId} in tenant {TenantId}",
            msg.EmployeeId, msg.TenantId);

        var notification = Notification.Create(
            msg.TenantId,
            msg.EmployeeId,
            NotificationChannel.InApp,
            NotificationCategory.Leave,
            NotificationPriority.High,
            "Leave Request Rejected",
            $"Your leave request has been rejected. Note: {msg.RejectionNote}",
            correlationId: msg.Id.ToString());

        await repository.AddAsync(notification, context.CancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}

public sealed class SubscriptionPastDueNotificationConsumer(
    INotificationRepository repository,
    ILogger<SubscriptionPastDueNotificationConsumer> logger) : IConsumer<SubscriptionPastDueIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionPastDueIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "Creating subscription past-due notification for tenant {TenantId}",
            msg.TenantId);

        var notification = Notification.Create(
            msg.TenantId,
            Guid.Empty,
            NotificationChannel.InApp,
            NotificationCategory.Billing,
            NotificationPriority.Critical,
            "Subscription Payment Overdue",
            "Your subscription payment is past due. Please update your payment method to avoid service interruption.",
            correlationId: msg.Id.ToString());

        await repository.AddAsync(notification, context.CancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}

public sealed class UserRegisteredNotificationConsumer(
    INotificationRepository repository,
    ILogger<UserRegisteredNotificationConsumer> logger) : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "Creating account-created notification for user {UserId} in tenant {TenantId}",
            msg.UserId, msg.TenantId);

        var notification = Notification.Create(
            msg.TenantId,
            msg.UserId,
            NotificationChannel.InApp,
            NotificationCategory.Security,
            NotificationPriority.Normal,
            "Account Created Successfully",
            "Welcome! Your account has been set up. Start by completing your profile.",
            correlationId: msg.Id.ToString());

        await repository.AddAsync(notification, context.CancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}
