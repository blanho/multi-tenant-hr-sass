using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.Modules.Leave.Domain.Entities;

public sealed class LeaveBalance : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public int Year { get; private set; }
    public int AnnualAllowance { get; private set; }
    public int SickAllowance { get; private set; }
    public int AnnualUsed { get; private set; }
    public int SickUsed { get; private set; }

    private LeaveBalance() { }

    public static LeaveBalance Seed(Guid tenantId, Guid employeeId, int year,
        int annualAllowance = 20, int sickAllowance = 10)
    {
        return new LeaveBalance
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            Year = year,
            AnnualAllowance = annualAllowance,
            SickAllowance = sickAllowance,
            AnnualUsed = 0,
            SickUsed = 0
        };
    }

    public int AnnualRemaining => AnnualAllowance - AnnualUsed;
    public int SickRemaining => SickAllowance - SickUsed;

    public void DeductAnnual(int days)
    {
        if (days <= 0) throw new DomainException("Days must be positive.");
        if (days > AnnualRemaining)
            throw new DomainException($"Insufficient annual leave balance. Remaining: {AnnualRemaining}, Requested: {days}.");

        AnnualUsed += days;
        Touch();
    }

    public void DeductSick(int days)
    {
        if (days <= 0) throw new DomainException("Days must be positive.");
        SickUsed = Math.Min(SickUsed + days, SickAllowance);
        Touch();
    }

    public void RestoreAnnual(int days)
    {
        AnnualUsed = Math.Max(0, AnnualUsed - days);
        Touch();
    }

    public void RestoreSick(int days)
    {
        SickUsed = Math.Max(0, SickUsed - days);
        Touch();
    }
}
