using HrSaas.Modules.Tenant.Domain.Events;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Tenant.Domain.Entities;

public enum PlanType { Free = 0, Starter = 1, Professional = 2, Enterprise = 3 }
public enum TenantStatus { Active = 0, Suspended = 1, Cancelled = 2, PendingSetup = 3 }

public sealed class Tenant : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string ContactEmail { get; private set; } = null!;
    public PlanType Plan { get; private set; }
    public TenantStatus Status { get; private set; }
    public int MaxEmployees { get; private set; }
    public DateTime? SuspendedAt { get; private set; }

    private static readonly IReadOnlyDictionary<PlanType, int> PlanLimits = new Dictionary<PlanType, int>
    {
        [PlanType.Free] = 10,
        [PlanType.Starter] = 50,
        [PlanType.Professional] = 250,
        [PlanType.Enterprise] = int.MaxValue
    };

    private Tenant() { }

    public static Tenant Create(string name, string slug, string contactEmail, PlanType plan = PlanType.Free)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.NotNullOrWhiteSpace(slug, nameof(slug));
        Guard.NotNullOrWhiteSpace(contactEmail, nameof(contactEmail));

        var tenant = new Tenant
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            ContactEmail = contactEmail.ToLowerInvariant(),
            Plan = plan,
            Status = TenantStatus.PendingSetup,
            MaxEmployees = PlanLimits[plan]
        };

        tenant.TenantId = tenant.Id;
        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, name, tenant.Slug, tenant.ContactEmail, plan.ToString()));
        return tenant;
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        Touch();
    }

    public void UpdateDetails(string name, string contactEmail)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.NotNullOrWhiteSpace(contactEmail, nameof(contactEmail));
        Name = name;
        ContactEmail = contactEmail.ToLowerInvariant();
        Touch();
    }

    public void Upgrade(PlanType newPlan)
    {
        if (newPlan <= Plan)
        {
            throw new InvalidOperationException("Can only upgrade to a higher plan.");
        }

        var oldPlan = Plan;
        Plan = newPlan;
        MaxEmployees = PlanLimits[newPlan];
        Touch();
        AddDomainEvent(new TenantPlanUpgradedEvent(TenantId, oldPlan.ToString(), newPlan.ToString(), MaxEmployees));
    }

    public void Suspend(string reason)
    {
        Guard.NotNullOrWhiteSpace(reason, nameof(reason));
        Status = TenantStatus.Suspended;
        SuspendedAt = DateTime.UtcNow;
        Touch();
        AddDomainEvent(new TenantSuspendedEvent(TenantId, reason));
    }

    public void Reinstate()
    {
        if (Status != TenantStatus.Suspended)
        {
            throw new InvalidOperationException("Only suspended tenants can be reinstated.");
        }

        Status = TenantStatus.Active;
        SuspendedAt = null;
        Touch();
        AddDomainEvent(new TenantReinstatedEvent(TenantId));
    }
}
