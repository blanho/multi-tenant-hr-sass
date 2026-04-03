#!/usr/bin/env python3
"""Generate MediatR behaviors, exception middleware, appsettings, and CI."""
import os

ROOT = "/Users/macbook/Desktop/multi-tenant-sass"

def write(rel_path, content):
    full = os.path.join(ROOT, rel_path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  {rel_path}")

# ─── MediatR Pipeline Behaviors ──────────────────────────────────────────────
write("src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/Behaviors/LoggingBehavior.cs", """\
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HrSaas.SharedKernel.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName}", requestName);

        TResponse response;
        try
        {
            response = await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Error handling {RequestName} after {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }

        sw.Stop();
        logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
        return response;
    }
}
""")

write("src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/Behaviors/ValidationBehavior.cs", """\
using FluentValidation;
using MediatR;

namespace HrSaas.SharedKernel.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}
""")

print("Behaviors done")

# ─── Exception Middleware ─────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/Middleware/ExceptionMiddleware.cs", """\
using FluentValidation;
using HrSaas.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HrSaas.Api.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed: {Errors}", ex.Errors);
            await WriteProblemDetailsAsync(context, HttpStatusCode.UnprocessableEntity, "Validation Failed",
                ex.Errors.Select(e => e.ErrorMessage).ToArray()).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Domain rule violation: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Domain Rule Violation", ex.Message).ConfigureAwait(false);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemDetailsAsync(context, HttpStatusCode.NotFound, "Not Found", ex.Message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblemDetailsAsync(context, HttpStatusCode.InternalServerError, "Internal Server Error",
                "An unexpected error occurred.").ConfigureAwait(false);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, HttpStatusCode statusCode, string title, params string[] errors)
    {
        var problem = new ProblemDetails
        {
            Title = title,
            Status = (int)statusCode,
            Instance = context.Request.Path
        };

        if (errors.Length > 0)
        {
            problem.Extensions["errors"] = errors;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem).ConfigureAwait(false);
    }
}
""")

print("Exception middleware done")

# ─── NotFoundException ────────────────────────────────────────────────────────
write("src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/Exceptions/NotFoundException.cs", """\
namespace HrSaas.SharedKernel.Exceptions;

public sealed class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with key '{key}' was not found.")
{
    public string EntityName { get; } = entityName;
    public object Key { get; } = key;
}
""")

print("NotFoundException done")

# ─── appsettings.json ─────────────────────────────────────────────────────────
write("src/Api/HrSaas.Api/appsettings.json", """\
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hrsaas;Username=postgres;Password=postgres",
    "RabbitMQ": "amqp://guest:guest@localhost:5672/",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Authority": "https://localhost:5001",
    "Audience": "hrsaas-api",
    "SecretKey": "CHANGE_ME_IN_PRODUCTION_USE_SECRETS_MANAGER",
    "Issuer": "HrSaas",
    "ExpiryMinutes": 60
  },
  "Telemetry": {
    "ServiceName": "HrSaas.Api",
    "OtlpEndpoint": "http://localhost:4317"
  },
  "AllowedHosts": "*"
}
""")

write("src/Api/HrSaas.Api/appsettings.Development.json", """\
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
""")

print("appsettings done")

# ─── GitHub Actions CI ────────────────────────────────────────────────────────
write(".github/workflows/ci.yml", """\
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: '9.0.x'
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true

jobs:
  build-and-test:
    name: Build & Test
    runs-on: ubuntu-latest

    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_USER: test
          POSTGRES_PASSWORD: test
          POSTGRES_DB: hrsaas_test
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore
        run: dotnet restore HrSaas.sln

      - name: Build
        run: dotnet build HrSaas.sln --no-restore --configuration Release

      - name: Unit Tests
        run: >
          dotnet test tests/HrSaas.Modules.Employee.UnitTests/HrSaas.Modules.Employee.UnitTests.csproj
          --no-build --configuration Release
          --logger "trx;LogFileName=unit-tests.trx"
          --collect "XPlat Code Coverage"

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: "**/TestResults/**/*.trx"

  lint:
    name: Format Check
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Format check
        run: dotnet format HrSaas.sln --verify-no-changes --severity warn
""")

print("CI workflow done")

# ─── Docker Compose update ────────────────────────────────────────────────────
write("docker-compose.yml", """\
services:
  postgres:
    image: postgres:16-alpine
    container_name: hrsaas-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: hrsaas
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: hrsaas-rabbitmq
    restart: unless-stopped
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    ports:
      - "5672:5672"
      - "15672:15672"
    healthcheck:
      test: rabbitmq-diagnostics -q ping
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: hrsaas-redis
    restart: unless-stopped
    ports:
      - "6379:6379"
    command: redis-server --appendonly yes
    volumes:
      - redis-data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    container_name: hrsaas-otel
    restart: unless-stopped
    command: ["--config=/etc/otel-collector-config.yml"]
    volumes:
      - ./infra/otel-collector-config.yml:/etc/otel-collector-config.yml
    ports:
      - "4317:4317"
      - "4318:4318"

  seq:
    image: datalust/seq:latest
    container_name: hrsaas-seq
    restart: unless-stopped
    environment:
      ACCEPT_EULA: Y
    ports:
      - "5341:5341"
      - "8080:80"

volumes:
  postgres-data:
  redis-data:
""")

write("infra/otel-collector-config.yml", """\
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    timeout: 10s
    send_batch_size: 1024
  memory_limiter:
    check_interval: 1s
    limit_mib: 512

exporters:
  debug:
    verbosity: detailed

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [debug]
    metrics:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [debug]
""")

print("Docker compose + OTel infra done")

print("\nAll remaining infrastructure generated.")
