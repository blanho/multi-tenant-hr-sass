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
        catch (TenantNotFoundException ex)
        {
            logger.LogWarning("Tenant not found: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, HttpStatusCode.Unauthorized, "Tenant Identification Required", ex.Message).ConfigureAwait(false);
        }
        catch (TenantAccessDeniedException ex)
        {
            logger.LogWarning("Tenant access denied: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, HttpStatusCode.Forbidden, "Access Denied", ex.Message).ConfigureAwait(false);
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

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        params string[] errors)
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
