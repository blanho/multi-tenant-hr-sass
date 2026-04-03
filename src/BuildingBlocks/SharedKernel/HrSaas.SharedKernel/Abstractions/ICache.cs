namespace HrSaas.SharedKernel.Abstractions;

public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
    Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
}

public static class CacheKeys
{
    public static string Employee(Guid tenantId, Guid employeeId) => $"tenant:{tenantId}:employee:{employeeId}";
    public static string EmployeeList(Guid tenantId) => $"tenant:{tenantId}:employees";
    public static string Tenant(Guid tenantId) => $"tenant:{tenantId}";
    public static string UserById(Guid tenantId, Guid userId) => $"tenant:{tenantId}:user:{userId}";
    public static string Subscription(Guid tenantId) => $"tenant:{tenantId}:subscription";
}
