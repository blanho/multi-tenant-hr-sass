using FluentValidation;
using HrSaas.Modules.Tenant.Application.DTOs;
using HrSaas.Modules.Tenant.Application.Interfaces;
using HrSaas.Modules.Tenant.Domain.Entities;
using HrSaas.SharedKernel.CQRS;
using MediatR;
using TenantEntity = HrSaas.Modules.Tenant.Domain.Entities.Tenant;

namespace HrSaas.Modules.Tenant.Application.Commands;

public sealed record CreateTenantCommand(string Name, string Slug, string ContactEmail, PlanType Plan = PlanType.Free) : ICommand<Guid>;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens.");
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(254);
    }
}

public sealed class CreateTenantCommandHandler(ITenantRepository repo) : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await repo.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result<Guid>.Failure("A tenant with this slug already exists.", "SLUG_TAKEN");
        }

        var tenant = TenantEntity.Create(request.Name, request.Slug, request.ContactEmail, request.Plan);
        tenant.Activate();
        await repo.AddAsync(tenant, cancellationToken).ConfigureAwait(false);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Guid>.Success(tenant.Id);
    }
}

public sealed record SuspendTenantCommand(Guid TenantId, string Reason) : ICommand;

public sealed class SuspendTenantCommandValidator : AbstractValidator<SuspendTenantCommand>
{
    public SuspendTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class SuspendTenantCommandHandler(ITenantRepository repo) : IRequestHandler<SuspendTenantCommand, Result>
{
    public async Task<Result> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await repo.GetByIdAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return Result.Failure("Tenant not found.", "NOT_FOUND");
        }

        tenant.Suspend(request.Reason);
        repo.Update(tenant);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record UpgradePlanCommand(Guid TenantId, PlanType NewPlan) : ICommand;

public sealed class UpgradePlanCommandValidator : AbstractValidator<UpgradePlanCommand>
{
    public UpgradePlanCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.NewPlan).IsInEnum();
    }
}

public sealed class UpgradePlanCommandHandler(ITenantRepository repo) : IRequestHandler<UpgradePlanCommand, Result>
{
    public async Task<Result> Handle(UpgradePlanCommand request, CancellationToken cancellationToken)
    {
        var tenant = await repo.GetByIdAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return Result.Failure("Tenant not found.", "NOT_FOUND");
        }

        tenant.Upgrade(request.NewPlan);
        repo.Update(tenant);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record ReinstateCommand(Guid TenantId) : ICommand;

public sealed class ReinstateCommandValidator : AbstractValidator<ReinstateCommand>
{
    public ReinstateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public sealed class ReinstateCommandHandler(ITenantRepository repo) : IRequestHandler<ReinstateCommand, Result>
{
    public async Task<Result> Handle(ReinstateCommand request, CancellationToken cancellationToken)
    {
        var tenant = await repo.GetByIdAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return Result.Failure("Tenant not found.", "NOT_FOUND");
        }

        try
        {
            tenant.Reinstate();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message, "INVALID_STATE");
        }

        repo.Update(tenant);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
