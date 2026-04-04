using HrSaas.SharedKernel.Jobs;

namespace HrSaas.Modules.Billing.Infrastructure.Jobs;

public sealed class BillingJobConfiguration : IRecurringJobConfiguration
{
    public IReadOnlyList<RecurringJobDefinition> GetRecurringJobs() =>
    [
        new RecurringJobDefinition(
            "billing:check-expiry",
            typeof(SubscriptionExpiryJob),
            "0 * * * *")
    ];
}
