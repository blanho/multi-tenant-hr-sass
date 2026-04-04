using FluentValidation;
using HrSaas.Modules.Storage.Domain.Repositories;
using HrSaas.Modules.Storage.Infrastructure.Persistence;
using HrSaas.Modules.Storage.Infrastructure.Repositories;
using HrSaas.Modules.Storage.Jobs;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Storage;

public static class StorageModule
{
    public static IServiceCollection AddStorageModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StorageDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_storage", "storage")));

        services.AddScoped<IStoredFileRepository, StoredFileRepository>();

        services.AddScoped<OrphanedFileCleanupJob>();
        services.AddSingleton<IRecurringJobConfiguration, StorageJobConfiguration>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(StorageModule).Assembly));

        services.AddValidatorsFromAssembly(typeof(StorageModule).Assembly);

        return services;
    }
}
