namespace HrSaas.SharedKernel.FeatureFlags;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class FeatureGateAttribute(string featureName) : Attribute
{
    public string FeatureName { get; } = featureName;
}
