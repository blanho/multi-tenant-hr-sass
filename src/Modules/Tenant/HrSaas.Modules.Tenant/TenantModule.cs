using FluentValidation;
using HrSaas.Modules.Tenant.Application.Interfaces;
using HrSaas.Modules.Tenant.Infrastructure.Persistence;
using HrSaas.Modules.Tenant.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Tenant;

public static class TenantModule
{
    public static IServiceCollection AddTenantModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TenantDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_tenant", "tenant")));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TenantModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(TenantModule).Assembly);

        return services;
    }
}
