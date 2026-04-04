using FluentValidation;
using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.Modules.Notifications.Infrastructure.Channels;
using HrSaas.Modules.Notifications.Infrastructure.Jobs;
using HrSaas.Modules.Notifications.Infrastructure.Persistence;
using HrSaas.Modules.Notifications.Infrastructure.Repositories;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_notification", "notification")));

        services.AddScoped<INotificationsDbContext>(sp =>
            sp.GetRequiredService<NotificationsDbContext>());

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<IUserNotificationPreferenceRepository, UserNotificationPreferenceRepository>();

        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<SlackOptions>(configuration.GetSection(SlackOptions.SectionName));

        services.AddScoped<IChannelProvider, EmailChannelProvider>();
        services.AddScoped<IChannelProvider, InAppChannelProvider>();
        services.AddScoped<IChannelProvider, WebhookChannelProvider>();
        services.AddScoped<IChannelProvider, SlackChannelProvider>();
        services.AddScoped<IChannelProviderFactory, ChannelProviderFactory>();

        services.AddHttpClient("NotificationWebhook", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "HrSaas-Notifications/1.0");
        });

        services.AddHttpClient("SlackNotification", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddScoped<ScheduledNotificationDispatchJob>();
        services.AddScoped<FailedNotificationRetryJob>();
        services.AddScoped<NotificationDigestJob>();
        services.AddScoped<ExpiredNotificationCleanupJob>();
        services.AddSingleton<IRecurringJobConfiguration, NotificationJobConfiguration>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(NotificationsModule).Assembly));

        services.AddValidatorsFromAssembly(typeof(NotificationsModule).Assembly);

        return services;
    }
}
