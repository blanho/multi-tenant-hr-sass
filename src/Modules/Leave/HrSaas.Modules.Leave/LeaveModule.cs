using FluentValidation;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Application.Policies;
using HrSaas.Modules.Leave.Infrastructure.Persistence;
using HrSaas.Modules.Leave.Infrastructure.Persistence.Repositories;
using HrSaas.Modules.Leave.Infrastructure.Policies;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Leave;

public static class LeaveModule
{
    public static IServiceCollection AddLeaveModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LeaveDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_leave", "leave")));

        services.AddScoped<ILeaveRepository, LeaveRepository>();
        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
        services.AddScoped<ILeaveBalancePolicy, DefaultLeaveBalancePolicy>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LeaveModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(LeaveModule).Assembly);

        return services;
    }
}
