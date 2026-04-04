using System.Security.Claims;
using HrSaas.SharedKernel.Audit;
using Microsoft.AspNetCore.Http;

namespace HrSaas.Api.Infrastructure.Audit;

public sealed class HttpAuditContext(IHttpContextAccessor httpContextAccessor) : IAuditContext
{
    public Guid? UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? UserEmail =>
        httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value
        ?? httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value;

    public Guid? TenantId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers.UserAgent.FirstOrDefault();

    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? httpContextAccessor.HttpContext?.TraceIdentifier;
}
