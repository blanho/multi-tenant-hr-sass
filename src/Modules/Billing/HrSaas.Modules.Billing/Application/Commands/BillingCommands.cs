using FluentValidation;
using HrSaas.Modules.Billing.Application.DTOs;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Domain.Entities;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Billing.Application.Commands;

public sealed record CreateFreeSubscriptionCommand(Guid TenantId) : ICommand<Guid>;

public sealed class CreateFreeSubscriptionCommandValidator : AbstractValidator<CreateFreeSubscriptionCommand>
{
    public CreateFreeSubscriptionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public sealed class CreateFreeSubscriptionCommandHandler(ISubscriptionRepository repo) : IRequestHandler<CreateFreeSubscriptionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateFreeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existing = await repo.GetActiveByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result<Guid>.Failure("Tenant already has an active subscription.", "ALREADY_SUBSCRIBED");
        }

        var subscription = Subscription.CreateFree(request.TenantId);
        await repo.AddAsync(subscription, cancellationToken).ConfigureAwait(false);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Guid>.Success(subscription.Id);
    }
}

public sealed record ActivateSubscriptionCommand(Guid TenantId, Guid SubscriptionId, decimal Price, BillingCycle Cycle, string? ExternalId) : ICommand;

public sealed class ActivateSubscriptionCommandValidator : AbstractValidator<ActivateSubscriptionCommand>
{
    public ActivateSubscriptionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Cycle).IsInEnum();
    }
}

public sealed class ActivateSubscriptionCommandHandler(ISubscriptionRepository repo) : IRequestHandler<ActivateSubscriptionCommand, Result>
{
    public async Task<Result> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var sub = await repo.GetByIdAsync(request.SubscriptionId, cancellationToken).ConfigureAwait(false);
        if (sub is null || sub.TenantId != request.TenantId)
        {
            return Result.Failure("Subscription not found.", "NOT_FOUND");
        }

        sub.Activate(request.Price, request.Cycle, request.ExternalId);
        repo.Update(sub);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record CancelSubscriptionCommand(Guid TenantId, Guid SubscriptionId, string Reason) : ICommand;

public sealed class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CancelSubscriptionCommandHandler(ISubscriptionRepository repo) : IRequestHandler<CancelSubscriptionCommand, Result>
{
    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var sub = await repo.GetByIdAsync(request.SubscriptionId, cancellationToken).ConfigureAwait(false);
        if (sub is null || sub.TenantId != request.TenantId)
        {
            return Result.Failure("Subscription not found.", "NOT_FOUND");
        }

        sub.Cancel(request.Reason);
        repo.Update(sub);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
