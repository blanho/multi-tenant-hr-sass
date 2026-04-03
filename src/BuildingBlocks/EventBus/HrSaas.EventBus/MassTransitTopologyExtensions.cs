using MassTransit;

namespace HrSaas.EventBus;

public static class MassTransitTopologyExtensions
{
    public static readonly IEndpointNameFormatter HrSaasEndpointNameFormatter =
        new KebabCaseEndpointNameFormatter("hr-saas", false);

    public static IRabbitMqBusFactoryConfigurator ApplyHrSaasTopology(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext ctx)
    {
        cfg.PrefetchCount = 16;

        cfg.UseMessageRetry(r =>
            r.Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(5),
                maxInterval: TimeSpan.FromMinutes(2),
                intervalDelta: TimeSpan.FromSeconds(5)));

        cfg.ConfigureEndpoints(ctx, HrSaasEndpointNameFormatter);

        return cfg;
    }
}
