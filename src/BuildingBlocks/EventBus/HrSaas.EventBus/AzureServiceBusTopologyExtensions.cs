using MassTransit;

namespace HrSaas.EventBus;

public static class AzureServiceBusTopologyExtensions
{
    public static IServiceBusBusFactoryConfigurator ApplyHrSaasTopology(
        this IServiceBusBusFactoryConfigurator cfg,
        IBusRegistrationContext ctx)
    {
        cfg.UseMessageRetry(r =>
            r.Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(5),
                maxInterval: TimeSpan.FromMinutes(2),
                intervalDelta: TimeSpan.FromSeconds(5)));

        cfg.ConfigureEndpoints(ctx, MassTransitTopologyExtensions.HrSaasEndpointNameFormatter);

        return cfg;
    }
}
