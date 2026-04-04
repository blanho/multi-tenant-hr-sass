using HrSaas.Modules.Audit.Infrastructure;
using HrSaas.Modules.Audit.Infrastructure.Persistence;
using HrSaas.Modules.Audit.Jobs;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Audit;

public static class AuditModule
{
    public static IServiceCollection AddAuditModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_audit", "audit")));

        services.AddScoped<IAuditLogStore, EfAuditLogStore>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AuditModule).Assembly));

        services.AddScoped<AuditLogRetentionJob>();
        services.AddSingleton<IRecurringJobConfiguration, AuditJobConfiguration>();

        return services;
    }
}
