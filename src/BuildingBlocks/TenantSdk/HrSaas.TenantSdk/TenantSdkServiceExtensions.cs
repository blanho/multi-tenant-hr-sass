using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.TenantSdk;

public static class TenantSdkServiceExtensions
{
    public static IServiceCollection AddTenantSdk(this IServiceCollection services)
    {
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantService, TenantService>();
        return services;
    }
}
