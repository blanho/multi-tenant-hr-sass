using HrSaas.Modules.Leave.Application.DTOs;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Leave.Application.Queries;

public sealed record GetLeaveByIdQuery(Guid TenantId, Guid LeaveRequestId) : IQuery<LeaveRequestDto>;

public sealed class GetLeaveByIdQueryHandler(ILeaveRepository repo) : IRequestHandler<GetLeaveByIdQuery, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(GetLeaveByIdQuery request, CancellationToken cancellationToken)
    {
        var leave = await repo.GetByIdForTenantAsync(request.LeaveRequestId, request.TenantId, cancellationToken).ConfigureAwait(false);
        if (leave is null)
        {
            return Result<LeaveRequestDto>.Failure("Leave request not found.", "NOT_FOUND");
        }

        return Result<LeaveRequestDto>.Success(new LeaveRequestDto(
            leave.Id, leave.TenantId, leave.EmployeeId, leave.Type.ToString(), leave.Status.ToString(),
            leave.StartDate, leave.EndDate, leave.Reason, leave.RejectionNote, leave.GetDurationDays(), leave.CreatedAt));
    }
}

public sealed record GetLeavesByEmployeeQuery(Guid TenantId, Guid EmployeeId) : IQuery<IReadOnlyList<LeaveRequestDto>>;

public sealed class GetLeavesByEmployeeQueryHandler(ILeaveRepository repo) : IRequestHandler<GetLeavesByEmployeeQuery, Result<IReadOnlyList<LeaveRequestDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetLeavesByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var leaves = await repo.GetByEmployeeAsync(request.TenantId, request.EmployeeId, cancellationToken).ConfigureAwait(false);
        var dtos = leaves.Select(Map).ToList().AsReadOnly();
        return Result<IReadOnlyList<LeaveRequestDto>>.Success(dtos);
    }

    private static LeaveRequestDto Map(Domain.Entities.LeaveRequest l) =>
        new(l.Id, l.TenantId, l.EmployeeId, l.Type.ToString(), l.Status.ToString(),
            l.StartDate, l.EndDate, l.Reason, l.RejectionNote, l.GetDurationDays(), l.CreatedAt);
}

public sealed record GetPendingLeavesQuery(Guid TenantId) : IQuery<IReadOnlyList<LeaveRequestDto>>;

public sealed class GetPendingLeavesQueryHandler(ILeaveRepository repo) : IRequestHandler<GetPendingLeavesQuery, Result<IReadOnlyList<LeaveRequestDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetPendingLeavesQuery request, CancellationToken cancellationToken)
    {
        var leaves = await repo.GetPendingAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        var dtos = leaves.Select(l =>
            new LeaveRequestDto(l.Id, l.TenantId, l.EmployeeId, l.Type.ToString(), l.Status.ToString(),
                l.StartDate, l.EndDate, l.Reason, l.RejectionNote, l.GetDurationDays(), l.CreatedAt))
            .ToList().AsReadOnly();
        return Result<IReadOnlyList<LeaveRequestDto>>.Success(dtos);
    }
}
