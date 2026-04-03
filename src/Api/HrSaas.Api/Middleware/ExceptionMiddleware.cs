using FluentValidation;
using HrSaas.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HrSaas.Api.Middleware;

public sealed class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),

            DomainException de => (
                StatusCodes.Status422UnprocessableEntity,
                "Business Rule Violation",
                de.Message),

            NotFoundException nfe => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                nfe.Message),

            TenantNotFoundException tfe => (
                StatusCodes.Status401Unauthorized,
                "Tenant Not Found",
                tfe.Message),

            TenantAccessDeniedException tde => (
                StatusCodes.Status403Forbidden,
                "Tenant Access Denied",
                tde.Message),

            OperationCanceledException => (
                StatusCodes.Status499ClientClosedRequest,
                "Request Cancelled",
                "The request was cancelled by the client."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "An internal server error occurred. Please try again later.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            logger.LogWarning(exception,
                "Handled exception ({ExceptionType}) on {Method} {Path}",
                exception.GetType().Name,
                context.Request.Method,
                context.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",


            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationEx)
        {
            problem.Extensions["errors"] = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
