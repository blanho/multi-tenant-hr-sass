using Hangfire;
using Hangfire.Common;
using Hangfire.PostgreSql;
using HrSaas.JobScheduler.Filters;
using HrSaas.SharedKernel.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HrSaas.JobScheduler;

public static class JobSchedulerServiceExtensions
{
    public static IServiceCollection AddJobScheduler(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(
                    configuration.GetConnectionString("DefaultConnection"))));

        services.AddHangfireServer(options =>
        {
            options.Queues = ["critical", "default", "notifications", "maintenance"];
            options.WorkerCount = Math.Max(Environment.ProcessorCount * 2, 4);
            options.ServerName = $"hrsaas-{Environment.MachineName}-{Guid.NewGuid().ToString("N")[..8]}";
        });

        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }

    public static IApplicationBuilder UseJobDashboard(
        this IApplicationBuilder app,
        bool requireAuth = true)
    {
        var dashboardOptions = new DashboardOptions
        {
            DashboardTitle = "HrSaas Job Dashboard",
            DisplayStorageConnectionString = false,
            StatsPollingInterval = 5000
        };

        if (requireAuth)
        {
            dashboardOptions.Authorization = [new DashboardAuthorizationFilter()];
        }

        app.UseHangfireDashboard("/hangfire", dashboardOptions);

        return app;
    }

    public static IApplicationBuilder RegisterRecurringJobs(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IRecurringJobConfiguration>>();
        var configurations = scope.ServiceProvider.GetServices<IRecurringJobConfiguration>();
        var manager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        var executeMethod = typeof(IRecurringJob).GetMethod(nameof(IRecurringJob.ExecuteAsync))
            ?? throw new InvalidOperationException("IRecurringJob.ExecuteAsync method not found");

        foreach (var config in configurations)
        {
            foreach (var definition in config.GetRecurringJobs())
            {
                var job = new Job(definition.JobType, executeMethod, CancellationToken.None);

                manager.AddOrUpdate(
                    definition.JobId,
                    job,
                    definition.CronExpression,
                    new RecurringJobOptions
                    {
                        QueueName = definition.Queue,
                        TimeZone = definition.TimeZone ?? TimeZoneInfo.Utc
                    });

                logger.LogInformation(
                    "Registered recurring job {JobId} ({JobType}) with schedule {Cron} on queue {Queue}",
                    definition.JobId, definition.JobType.Name, definition.CronExpression, definition.Queue);
            }
        }

        return app;
    }
}
