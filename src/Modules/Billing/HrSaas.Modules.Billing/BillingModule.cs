using FluentValidation;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Application.Policies;
using HrSaas.Modules.Billing.Infrastructure.Jobs;
using HrSaas.Modules.Billing.Infrastructure.Persistence;
using HrSaas.Modules.Billing.Infrastructure.Persistence.Repositories;
using HrSaas.Modules.Billing.Infrastructure.Policies;
using HrSaas.SharedKernel.Jobs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Billing;

public static class BillingModule
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BillingDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_billing", "billing")));

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IBillingPolicy, DefaultBillingPolicy>();

        services.AddScoped<SubscriptionExpiryJob>();
        services.AddSingleton<IRecurringJobConfiguration, BillingJobConfiguration>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BillingModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(BillingModule).Assembly);

        return services;
    }
}
