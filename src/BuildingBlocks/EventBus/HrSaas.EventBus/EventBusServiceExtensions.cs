using HrSaas.EventBus.Outbox;
using HrSaas.SharedKernel.Events;
using HrSaas.SharedKernel.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.EventBus;

public static class EventBusServiceExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OutboxDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_outbox", "outbox")));

        services.AddScoped<IOutboxStore, EfOutboxStore<OutboxDbContext>>();
        services.AddScoped<IEventBus, OutboxPublisher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
