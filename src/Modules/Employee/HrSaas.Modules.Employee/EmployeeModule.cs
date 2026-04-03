using FluentValidation;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.Modules.Employee.Infrastructure.Persistence;
using HrSaas.Modules.Employee.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Employee;

public static class EmployeeModule
{
    public static IServiceCollection AddEmployeeModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<EmployeeDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_employee", "employee")));

        services.AddScoped<IEmployeeDbContext>(sp => sp.GetRequiredService<EmployeeDbContext>());
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(EmployeeModule).Assembly));

        services.AddValidatorsFromAssembly(typeof(EmployeeModule).Assembly);

        return services;
    }
}
