using HrSaas.Modules.Leave.Domain.Events;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Leave.Domain.Entities;

public enum LeaveType { Annual = 0, Sick = 1, Maternity = 2, Paternity = 3, Unpaid = 4, Emergency = 5 }
public enum LeaveStatus { Pending = 0, Approved = 1, Rejected = 2, Cancelled = 3 }

public sealed class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public LeaveType Type { get; private set; }
    public LeaveStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Reason { get; private set; } = null!;
    public string? RejectionNote { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? DecisionAt { get; private set; }

    private LeaveRequest() { }

    public static LeaveRequest Apply(Guid tenantId, Guid employeeId, LeaveType type, DateTime start, DateTime end, string reason)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotEmpty(employeeId, nameof(employeeId));
        Guard.NotNullOrWhiteSpace(reason, nameof(reason));

        if (end <= start)
        {
            throw new DomainException("End date must be after start date.");
        }

        var request = new LeaveRequest
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            Type = type,
            Status = LeaveStatus.Pending,
            StartDate = start,
            EndDate = end,
            Reason = reason
        };

        request.AddDomainEvent(new LeaveAppliedEvent(tenantId, request.Id, employeeId, type.ToString()));
        return request;
    }

    public void Approve(Guid approvedByUserId)
    {
        if (Status != LeaveStatus.Pending)
        {
            throw new DomainException($"Cannot approve a leave request in {Status} status.");
        }

        Status = LeaveStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        DecisionAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LeaveApprovedEvent(TenantId, Id, EmployeeId, approvedByUserId));
    }

    public void Reject(Guid rejectedByUserId, string note)
    {
        if (Status != LeaveStatus.Pending)
        {
            throw new DomainException($"Cannot reject a leave request in {Status} status.");
        }

        Status = LeaveStatus.Rejected;
        RejectionNote = note;
        DecisionAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LeaveRejectedEvent(TenantId, Id, EmployeeId, note));
    }

    public void Cancel()
    {
        if (Status == LeaveStatus.Approved || Status == LeaveStatus.Rejected)
        {
            throw new DomainException("Cannot cancel an already decided leave request.");
        }

        Status = LeaveStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetDurationDays() => (int)(EndDate - StartDate).TotalDays + 1;
}
