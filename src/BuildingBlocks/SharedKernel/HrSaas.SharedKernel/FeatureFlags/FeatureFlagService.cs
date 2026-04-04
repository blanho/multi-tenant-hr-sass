using Microsoft.FeatureManagement;

namespace HrSaas.SharedKernel.FeatureFlags;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(CancellationToken cancellationToken = default);
}

public sealed class FeatureFlagService(IFeatureManager featureManager) : IFeatureFlagService
{
    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        return await featureManager.IsEnabledAsync(featureName).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(CancellationToken cancellationToken = default)
    {
        var allFlags = new[]
        {
            FeatureFlags.BulkEmployeeImport,
            FeatureFlags.AdvancedReporting,
            FeatureFlags.LeaveApprovalWorkflow,
            FeatureFlags.EmailNotifications,
            FeatureFlags.SlackIntegration,
            FeatureFlags.AuditLog,
            FeatureFlags.CustomRoles,
            FeatureFlags.MultiCurrencyBilling,
            FeatureFlags.EmployeeSelfService,
            FeatureFlags.ApiWebhooks
        };

        var enabled = new List<string>();
        foreach (var flag in allFlags)
        {
            if (await featureManager.IsEnabledAsync(flag).ConfigureAwait(false))
                enabled.Add(flag);
        }

        return enabled.AsReadOnly();
    }
}
