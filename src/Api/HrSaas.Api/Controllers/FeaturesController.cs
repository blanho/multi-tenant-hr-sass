using Asp.Versioning;
using HrSaas.SharedKernel.FeatureFlags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrSaas.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public sealed class FeaturesController(IFeatureFlagService featureFlagService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnabledFeatures(CancellationToken ct)
    {
        var features = await featureFlagService.GetEnabledFeaturesAsync(ct);
        return Ok(features);
    }

    [HttpGet("{featureName}")]
    [ProducesResponseType<FeatureStatusResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatureStatus(string featureName, CancellationToken ct)
    {
        var isEnabled = await featureFlagService.IsEnabledAsync(featureName, ct);
        return Ok(new FeatureStatusResponse(featureName, isEnabled));
    }
}

public sealed record FeatureStatusResponse(string FeatureName, bool IsEnabled);
