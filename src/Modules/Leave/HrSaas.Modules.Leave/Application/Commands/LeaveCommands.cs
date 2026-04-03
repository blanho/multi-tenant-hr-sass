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

public sealed class ApproveLeaveCommandValidator : AbstractValidator<ApproveLeaveCommand>
{
    public ApproveLeaveCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LeaveRequestId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
    }
}

public sealed class ApproveLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<ApproveLeaveCommand, Result>
{
    public async Task<Result> Handle(ApproveLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await repo.GetByIdForTenantAsync(request.LeaveRequestId, request.TenantId, cancellationToken).ConfigureAwait(false);
        if (leave is null)
        {
            return Result.Failure("Leave request not found.", "NOT_FOUND");
        }

        try
        {
            leave.Approve(request.ApprovedByUserId);
        }
        catch (SharedKernel.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message, "INVALID_STATE");
        }

        repo.Update(leave);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record RejectLeaveCommand(Guid TenantId, Guid LeaveRequestId, Guid RejectedByUserId, string Note) : ICommand;

public sealed class RejectLeaveCommandValidator : AbstractValidator<RejectLeaveCommand>
{
    public RejectLeaveCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LeaveRequestId).NotEmpty();
        RuleFor(x => x.RejectedByUserId).NotEmpty();
        RuleFor(x => x.Note).NotEmpty().MaximumLength(1000);
    }
}

public sealed class RejectLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<RejectLeaveCommand, Result>
{
    public async Task<Result> Handle(RejectLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await repo.GetByIdForTenantAsync(request.LeaveRequestId, request.TenantId, cancellationToken).ConfigureAwait(false);
        if (leave is null)
        {
            return Result.Failure("Leave request not found.", "NOT_FOUND");
        }

        try
        {
            leave.Reject(request.RejectedByUserId, request.Note);
        }
        catch (SharedKernel.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message, "INVALID_STATE");
        }

        repo.Update(leave);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record CancelLeaveCommand(Guid TenantId, Guid LeaveRequestId, Guid EmployeeId) : ICommand;

public sealed class CancelLeaveCommandValidator : AbstractValidator<CancelLeaveCommand>
{
    public CancelLeaveCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LeaveRequestId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public sealed class CancelLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<CancelLeaveCommand, Result>
{
    public async Task<Result> Handle(CancelLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await repo.GetByIdForTenantAsync(request.LeaveRequestId, request.TenantId, cancellationToken).ConfigureAwait(false);
        if (leave is null)
        {
            return Result.Failure("Leave request not found.", "NOT_FOUND");
        }

        if (leave.EmployeeId != request.EmployeeId)
        {
            return Result.Failure("Only the owner can cancel their leave request.", "FORBIDDEN");
        }

        try
        {
            leave.Cancel(request.EmployeeId);
        }
        catch (SharedKernel.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message, "INVALID_STATE");
        }

        repo.Update(leave);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
