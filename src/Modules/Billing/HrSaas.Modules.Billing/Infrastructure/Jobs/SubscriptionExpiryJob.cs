using HrSaas.Modules.Billing.Domain.Entities;
using HrSaas.Modules.Billing.Infrastructure.Persistence;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Billing.Infrastructure.Jobs;

public sealed class SubscriptionExpiryJob(
    BillingDbContext dbContext,
    ILogger<SubscriptionExpiryJob> logger) : IRecurringJob
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var expiredTrialCount = 0;
        var pastDueCount = 0;

        var expiredTrials = await dbContext.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.Status == SubscriptionStatus.Trial
                && s.TrialEndsAt.HasValue
                && s.TrialEndsAt <= now
                && !s.IsDeleted)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        foreach (var subscription in expiredTrials)
        {
            subscription.Expire();
            expiredTrialCount++;
        }

        var expiredPeriods = await dbContext.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.Status == SubscriptionStatus.Active
                && s.CurrentPeriodEnd.HasValue
                && s.CurrentPeriodEnd <= now
                && !s.IsDeleted)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        foreach (var subscription in expiredPeriods)
        {
            subscription.MarkPastDue();
            pastDueCount++;
        }

        if (expiredTrialCount > 0 || pastDueCount > 0)
        {
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

            logger.LogInformation(
                "Subscription expiry check: {TrialExpired} trials expired, {PeriodExpired} subscriptions marked past due",
                expiredTrialCount, pastDueCount);
        }
    }
}
