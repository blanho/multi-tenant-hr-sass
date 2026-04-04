using HrSaas.SharedKernel.FeatureFlags;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace HrSaas.Api.Infrastructure.FeatureManagement;

public static class FeatureManagementExtensions
{
    public static WebApplicationBuilder AddFeatureFlags(this WebApplicationBuilder builder)
    {
        var appConfigConnectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");
        var label = builder.Environment.EnvironmentName;
        var sentinelKey = builder.Configuration["Azure:AppConfiguration:SentinelKey"] ?? "FeatureManagement:Sentinel";
        var refreshSeconds = builder.Configuration.GetValue<int?>("Azure:AppConfiguration:RefreshSeconds") ?? 30;

        if (!string.IsNullOrWhiteSpace(appConfigConnectionString))
        {
            builder.Configuration.AddAzureAppConfiguration(opts =>
            {
                opts.Connect(appConfigConnectionString)
                    .Select(KeyFilter.Any, LabelFilter.Null)
                    .Select(KeyFilter.Any, label)
                    .ConfigureRefresh(refresh =>
                    {
                        refresh.Register(sentinelKey, label: label, refreshAll: true)
                               .SetRefreshInterval(TimeSpan.FromSeconds(refreshSeconds));
                        refresh.Register(sentinelKey, label: LabelFilter.Null, refreshAll: true)
                               .SetRefreshInterval(TimeSpan.FromSeconds(refreshSeconds));
                    })
                    .UseFeatureFlags(flagOpts =>
                    {
                        flagOpts.Select(KeyFilter.Any, LabelFilter.Null);
                        flagOpts.Select(KeyFilter.Any, label);
                        flagOpts.SetRefreshInterval(TimeSpan.FromSeconds(refreshSeconds));
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
