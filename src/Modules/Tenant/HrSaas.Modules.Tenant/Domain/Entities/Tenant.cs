using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.Modules.Tenant.Domain.Entities;

public enum PlanType { Free, Starter, Professional, Enterprise }

public enum TenantStatus { Active, Suspended, Cancelled }

public sealed class Tenant : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Subdomain { get; private set; } = default!;
    public PlanType Plan { get; private set; }
    public TenantStatus Status { get; private set; }
    public string? ContactEmail { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name, string subdomain, string contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tenant name is required.");
        if (string.IsNullOrWhiteSpace(subdomain)) throw new DomainException("Subdomain is required.");

        var tenant = new Tenant
        {
            Name = name.Trim(),
            Subdomain = subdomain.Trim().ToLowerInvariant(),
            Plan = PlanType.Free,
            Status = TenantStatus.Active,
            ContactEmail = contactEmail?.Trim().ToLowerInvariant()
        };

        tenant.TenantId = tenant.Id;
        return tenant;
    }

    public void Upgrade(PlanType newPlan) { Plan = newPlan; Touch(); }
    public void Suspend() { Status = TenantStatus.Suspended; Touch(); }
    public void Activate() { Status = TenantStatus.Active; Touch(); }
}
