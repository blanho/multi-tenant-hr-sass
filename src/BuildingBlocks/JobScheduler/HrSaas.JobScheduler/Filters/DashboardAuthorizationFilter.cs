using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace HrSaas.JobScheduler.Filters;

public sealed class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
    }
}
