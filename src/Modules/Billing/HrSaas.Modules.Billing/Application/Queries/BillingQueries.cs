using HrSaas.Modules.Billing.Application.DTOs;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Billing.Application.Queries;

public sealed record GetSubscriptionByTenantQuery(Guid TenantId) : IQuery<SubscriptionDto>;

public sealed class GetSubscriptionByTenantQueryHandler(ISubscriptionRepository repo) : IRequestHandler<GetSubscriptionByTenantQuery, Result<SubscriptionDto>>
{
    public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionByTenantQuery request, CancellationToken cancellationToken)
    {
        var sub = await repo.GetActiveByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (sub is null)
        {
            return Result<SubscriptionDto>.Failure("No active subscription found.", "NOT_FOUND");
        }

        return Result<SubscriptionDto>.Success(new SubscriptionDto(
            sub.Id, sub.TenantId, sub.PlanName, sub.Status.ToString(), sub.Cycle.ToString(),
            sub.PricePerCycle, sub.MaxSeats, sub.UsedSeats, sub.TrialEndsAt, sub.CurrentPeriodEnd, sub.CreatedAt));
    }
}
