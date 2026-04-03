namespace HrSaas.Modules.Billing.Application.DTOs;

public sealed record SubscriptionDto(
    Guid Id,
    Guid TenantId,
    string PlanName,
    string Status,
    string BillingCycle,
    decimal PricePerCycle,
    int MaxSeats,
    int UsedSeats,
    DateTime? TrialEndsAt,
    DateTime? CurrentPeriodEnd,
    DateTime CreatedAt);
