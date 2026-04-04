using Azure.Identity;
using HrSaas.SharedKernel.FeatureFlags;
using Microsoft.FeatureManagement;

namespace HrSaas.Api.Infrastructure.FeatureManagement;

public static class FeatureManagementExtensions
{
    public static WebApplicationBuilder AddFeatureFlags(this WebApplicationBuilder builder)
    {
        var appConfigConnectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");

        if (!string.IsNullOrWhiteSpace(appConfigConnectionString))
        {
            builder.Configuration.AddAzureAppConfiguration(opts =>
            {
                opts.Connect(appConfigConnectionString)
                    .UseFeatureFlags(flagOpts =>
                    {
                        flagOpts.SetRefreshInterval(TimeSpan.FromSeconds(30));
                    });
            });

            builder.Services.AddAzureAppConfiguration();
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddFeatureManagement()
            .AddFeatureFilter<TenantFeatureFilter>();

        builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();

        return builder;
    }

    public static WebApplication UseFeatureFlags(this WebApplication app)
    {
        var appConfigConnectionString = app.Configuration.GetConnectionString("AzureAppConfiguration");

        if (!string.IsNullOrWhiteSpace(appConfigConnectionString))
        {
            app.UseAzureAppConfiguration();
        }

        return app;
    }
}
