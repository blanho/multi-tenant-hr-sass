namespace HrSaas.Modules.Billing.Application.Policies;

public interface IBillingPolicy
{
    int GetTrialDays(string plan);
    int GetMaxSeatsForPlan(string plan);
    string GetDefaultTrialPlan();
}
