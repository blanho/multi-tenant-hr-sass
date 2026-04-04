using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace HrSaas.Api.Infrastructure.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi("v1", opts =>
        {
            opts.AddDocumentTransformer((doc, ctx, ct) =>
            {
                doc.Info = new OpenApiInfo
                {
                    Title = "HrSaas API",
                    Version = "v1",
                    Description = "Multi-Tenant SaaS HR Management System API. " +
                                  "All endpoints require tenant identification via JWT claim (tenant_id) " +
                                  "or X-Tenant-ID header. Role-based access control enforces permission checks per operation.",
                    Contact = new OpenApiContact
                    {
                        Name = "HrSaas Engineering",
                        Email = "engineering@hrsaas.io"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Proprietary"
                    }
                };

                doc.Servers =
                [
                    new OpenApiServer { Url = "https://localhost:5001", Description = "Local Development" },
                    new OpenApiServer { Url = "https://hrsaas-staging-api.azurewebsites.net", Description = "Staging" },
                    new OpenApiServer { Url = "https://hrsaas-prod-api.azurewebsites.net", Description = "Production" }
                ];

                return Task.CompletedTask;
            });

            opts.AddDocumentTransformer<SecuritySchemeTransformer>();
            opts.AddOperationTransformer<TenantHeaderTransformer>();
        });

        return services;
    }
}

internal sealed class SecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token. The token must contain 'tenant_id' and 'role' claims."
        };

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    }
}

internal sealed class TenantHeaderTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var allowAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .Any(m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute);

        if (allowAnonymous)
            return Task.CompletedTask;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-ID",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Tenant identifier (fallback when JWT tenant_id claim is not present)",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid"
            }
        });

        return Task.CompletedTask;
    }
}
