using HrSaas.Modules.Billing.Application.Policies;

namespace HrSaas.Modules.Billing.Infrastructure.Policies;

public sealed class DefaultBillingPolicy : IBillingPolicy
{
    private static readonly IReadOnlyDictionary<string, (int TrialDays, int MaxSeats)> PlanConfig =
        new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Free"] = (0, 5),
            ["Starter"] = (14, 10),
            ["Professional"] = (30, 50),
            ["Enterprise"] = (30, 500)
        };

    public int GetTrialDays(string plan) =>
        PlanConfig.TryGetValue(plan, out var cfg) ? cfg.TrialDays : 14;

    public int GetMaxSeatsForPlan(string plan) =>
        PlanConfig.TryGetValue(plan, out var cfg) ? cfg.MaxSeats : 10;

    public string GetDefaultTrialPlan() => "Professional";
}
