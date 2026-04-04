using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement;

namespace HrSaas.Api.Infrastructure.FeatureManagement;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class FeatureGateAttribute(string featureName) : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();

        if (!await featureManager.IsEnabledAsync(featureName).ConfigureAwait(false))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Title = "Feature Not Available",
                Detail = $"The feature '{featureName}' is not enabled for your tenant.",
                Status = StatusCodes.Status404NotFound,
                Instance = context.HttpContext.Request.Path
            })
            {
                StatusCode = StatusCodes.Status404NotFound,
                ContentTypes = { "application/problem+json" }
            };
            return;
        }

        await next().ConfigureAwait(false);
    }
}
