using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.Modules.Leave.Domain.Entities;

public enum LeaveType { Annual, Sick, Personal, Maternity, Paternity, Unpaid }
public enum LeaveStatus { Pending, Approved, Rejected, Cancelled }

public sealed class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string Reason { get; private set; } = default!;
    public LeaveStatus Status { get; private set; }
    public string? ManagerNotes { get; private set; }
    public Guid? ReviewedByManagerId { get; private set; }

    private LeaveRequest() { }

    public static LeaveRequest Apply(
        Guid tenantId,
        Guid employeeId,
        LeaveType leaveType,
        DateOnly startDate,
        DateOnly endDate,
        string reason)
    {
        if (endDate < startDate) throw new DomainException("End date must be after start date.");
        if (startDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Leave cannot be applied for past dates.");

        return new LeaveRequest
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            LeaveType = leaveType,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason.Trim(),
            Status = LeaveStatus.Pending
        };
    }

    public void Approve(Guid managerId, string? notes = null)
    {
        if (Status != LeaveStatus.Pending)
            throw new DomainException("Only pending requests can be approved.");

        Status = LeaveStatus.Approved;
        ReviewedByManagerId = managerId;
        ManagerNotes = notes;
        Touch();
        AddDomainEvent(new Events.LeaveApprovedEvent(TenantId, Id, EmployeeId));
    }

    public void Reject(Guid managerId, string notes)
    {
        if (Status != LeaveStatus.Pending)
            throw new DomainException("Only pending requests can be rejected.");

        Status = LeaveStatus.Rejected;
        ReviewedByManagerId = managerId;
        ManagerNotes = notes;
        Touch();
        AddDomainEvent(new Events.LeaveRejectedEvent(TenantId, Id, EmployeeId));
    }

    public void Cancel()
    {
        if (Status is LeaveStatus.Approved or LeaveStatus.Rejected)
            throw new DomainException("Approved or rejected requests cannot be cancelled.");

        Status = LeaveStatus.Cancelled;
        Touch();
    }

    public int GetDurationDays() =>
        EndDate.DayNumber - StartDate.DayNumber + 1;
}
