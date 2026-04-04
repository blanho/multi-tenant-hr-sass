using HrSaas.Api.Infrastructure.Authorization;
using HrSaas.Api.Infrastructure.Azure;
using HrSaas.Api.Infrastructure.FeatureManagement;
using HrSaas.Api.Infrastructure.HealthChecks;
using HrSaas.Api.Infrastructure.Idempotency;
using HrSaas.Api.Infrastructure.Observability;
using HrSaas.Api.Infrastructure.OpenApi;
using HrSaas.Api.Infrastructure.RateLimiting;
using HrSaas.Api.Infrastructure.Versioning;
using HrSaas.EventBus;
using HrSaas.JobScheduler;
using HrSaas.Modules.Billing;
using HrSaas.Modules.Employee;
using HrSaas.Modules.Identity;
using HrSaas.Modules.Leave;
using HrSaas.Modules.Notifications;
using HrSaas.Modules.Tenant;
using HrSaas.SharedKernel.Behaviors;
using HrSaas.SharedKernel.Interceptors;
using HrSaas.SharedKernel.Storage;
using HrSaas.TenantSdk;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddAzureKeyVault();
    builder.AddFeatureFlags();

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId());

    builder.Services.AddTelemetry(builder.Configuration);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.Authority = builder.Configuration["Jwt:Authority"];
            opts.Audience = builder.Configuration["Jwt:Audience"];
            opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        });

    builder.Services.AddAuthorization();
    builder.Services.AddPermissionAuthorization();

    builder.Services.AddTenantSdk();

    builder.Services.AddScoped<AuditableEntityInterceptor>();
    builder.Services.AddScoped<DomainEventDispatcherInterceptor>();

    builder.Services.AddEventBus(builder.Configuration);
    builder.Services.AddJobScheduler(builder.Configuration);

    builder.Services.AddEmployeeModule(builder.Configuration);
    builder.Services.AddIdentityModule(builder.Configuration);
    builder.Services.AddTenantModule(builder.Configuration);
    builder.Services.AddLeaveModule(builder.Configuration);
    builder.Services.AddBillingModule(builder.Configuration);
    builder.Services.AddNotificationsModule(builder.Configuration);

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    builder.Services.AddStackExchangeRedisCache(opts =>
        opts.Configuration = builder.Configuration.GetConnectionString("Redis"));

    builder.Services.AddAzureBlobStorage(builder.Configuration);

    var useAzureServiceBus = !string.IsNullOrWhiteSpace(
        builder.Configuration.GetConnectionString("AzureServiceBus"));

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumers(
            typeof(BillingModule).Assembly,
            typeof(EmployeeModule).Assembly,
            typeof(IdentityModule).Assembly,
            typeof(LeaveModule).Assembly,
            typeof(TenantModule).Assembly,
            typeof(NotificationsModule).Assembly);

        if (useAzureServiceBus)
        {
            x.UsingAzureServiceBus((ctx, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("AzureServiceBus"));
                cfg.ApplyHrSaasTopology(ctx);
            });
        }
        else
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
                cfg.ApplyHrSaasTopology(ctx);
            });
        }
    });

    builder.Services.AddApiVersioningSetup();
    builder.Services.AddTenantRateLimiting();
    builder.Services.AddModuleHealthChecks(builder.Configuration);

    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()));

    builder.Services.AddSignalR();
    builder.Services.AddControllers();
    builder.Services.AddOpenApiDocumentation();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            await scope.ServiceProvider
                .GetRequiredService<HrSaas.Modules.Employee.Infrastructure.Persistence.EmployeeDbContext>()
                .Database.MigrateAsync();
            await scope.ServiceProvider
                .GetRequiredService<HrSaas.Modules.Identity.Infrastructure.Persistence.IdentityDbContext>()
                .Database.MigrateAsync();
            await scope.ServiceProvider
                .GetRequiredService<HrSaas.Modules.Tenant.Infrastructure.Persistence.TenantDbContext>()
                .Database.MigrateAsync();
            await scope.ServiceProvider
                .GetRequiredService<HrSaas.Modules.Leave.Infrastructure.Persistence.LeaveDbContext>()
                .Database.MigrateAsync();
            await scope.ServiceProvider
                .GetRequiredService<HrSaas.Modules.Billing.Infrastructure.Persistence.BillingDbContext>()
                .Database.MigrateAsync();
            await scope.ServiceProvider
                .GetRequiredService<HrSaas.Modules.Notifications.Infrastructure.Persistence.NotificationsDbContext>()
                .Database.MigrateAsync();
            logger.LogInformation("All database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations");
            throw;
        }
    }

    app.UseSerilogRequestLogging();
    app.UseFeatureFlags();
    app.UseMiddleware<HrSaas.Api.Middleware.ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(opts =>
        {
            opts.WithTitle("HrSaas API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseMiddleware<TenantMiddleware>();
    app.UseMiddleware<IdempotencyMiddleware>();

    app.UseJobDashboard(requireAuth: !app.Environment.IsDevelopment());
    app.RegisterRecurringJobs();

    app.MapControllers().RequireRateLimiting("api");
    app.MapHub<HrSaas.Api.Hubs.NotificationHub>("/hubs/notifications");
    app.MapHealthCheckEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
