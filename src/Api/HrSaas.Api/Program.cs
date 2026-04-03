using HrSaas.Api.Infrastructure.HealthChecks;
using HrSaas.Api.Infrastructure.Idempotency;
using HrSaas.Api.Infrastructure.Observability;
using HrSaas.Api.Infrastructure.RateLimiting;
using HrSaas.Api.Infrastructure.Versioning;
using HrSaas.EventBus;
using HrSaas.Modules.Billing;
using HrSaas.Modules.Employee;
using HrSaas.Modules.Identity;
using HrSaas.Modules.Leave;
using HrSaas.Modules.Tenant;
using HrSaas.SharedKernel.Behaviors;
using HrSaas.SharedKernel.Interceptors;
using HrSaas.TenantSdk;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

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

    builder.Services.AddTenantSdk();

    builder.Services.AddScoped<AuditableEntityInterceptor>();
    builder.Services.AddScoped<DomainEventDispatcherInterceptor>();

    builder.Services.AddEventBus(builder.Configuration);

    builder.Services.AddEmployeeModule(builder.Configuration);
    builder.Services.AddIdentityModule(builder.Configuration);
    builder.Services.AddTenantModule(builder.Configuration);
    builder.Services.AddLeaveModule(builder.Configuration);
    builder.Services.AddBillingModule(builder.Configuration);

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    builder.Services.AddStackExchangeRedisCache(opts =>
        opts.Configuration = builder.Configuration.GetConnectionString("Redis"));

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumers(
            typeof(BillingModule).Assembly,
            typeof(EmployeeModule).Assembly,
            typeof(IdentityModule).Assembly,
            typeof(LeaveModule).Assembly,
            typeof(TenantModule).Assembly);

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
            cfg.ApplyHrSaasTopology(ctx);
        });
    });

    builder.Services.AddApiVersioningSetup();
    builder.Services.AddTenantRateLimiting();
    builder.Services.AddModuleHealthChecks(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseMiddleware<HrSaas.Api.Middleware.ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseMiddleware<TenantMiddleware>();
    app.UseMiddleware<IdempotencyMiddleware>();

    app.MapControllers().RequireRateLimiting("api");
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
