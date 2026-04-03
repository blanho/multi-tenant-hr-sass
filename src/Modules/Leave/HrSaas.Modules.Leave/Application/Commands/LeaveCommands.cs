using FluentValidation;
using HrSaas.Modules.Leave.Application.DTOs;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Domain.Entities;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Leave.Application.Commands;

public sealed record ApplyLeaveCommand(
    Guid TenantId,
    Guid EmployeeId,
    LeaveType Type,
    DateTime StartDate,
    DateTime EndDate,
    string Reason) : ICommand<Guid>;

public sealed class ApplyLeaveCommandValidator : AbstractValidator<ApplyLeaveCommand>
{
    public ApplyLeaveCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");
        RuleFor(x => x.StartDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Cannot apply leave in the past.");
    }
}

public sealed class ApplyLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<ApplyLeaveCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ApplyLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = LeaveRequest.Apply(request.TenantId, request.EmployeeId, request.Type, request.StartDate, request.EndDate, request.Reason);
        await repo.AddAsync(leave, cancellationToken).ConfigureAwait(false);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Guid>.Success(leave.Id);
    }
}

public sealed record ApproveLeaveCommand(Guid TenantId, Guid LeaveRequestId, Guid ApprovedByUserId) : ICommand;

public sealed class ApproveLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<ApproveLeaveCommand, Result>
{
    public async Task<Result> Handle(ApproveLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await repo.GetByIdAsync(request.LeaveRequestId, cancellationToken).ConfigureAwait(false);
        if (leave is null || leave.TenantId != request.TenantId)
        {
            return Result.Failure("Leave request not found.", "NOT_FOUND");
        }

        leave.Approve(request.ApprovedByUserId);
        repo.Update(leave);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record RejectLeaveCommand(Guid TenantId, Guid LeaveRequestId, Guid RejectedByUserId, string Note) : ICommand;

public sealed class RejectLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<RejectLeaveCommand, Result>
{
    public async Task<Result> Handle(RejectLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await repo.GetByIdAsync(request.LeaveRequestId, cancellationToken).ConfigureAwait(false);
        if (leave is null || leave.TenantId != request.TenantId)
        {
            return Result.Failure("Leave request not found.", "NOT_FOUND");
        }

        leave.Reject(request.RejectedByUserId, request.Note);
        repo.Update(leave);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
