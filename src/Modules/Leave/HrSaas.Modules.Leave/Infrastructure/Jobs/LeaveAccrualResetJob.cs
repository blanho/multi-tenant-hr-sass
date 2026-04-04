using HrSaas.Modules.Leave.Domain.Entities;
using HrSaas.Modules.Leave.Infrastructure.Persistence;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Leave.Infrastructure.Jobs;

public sealed class LeaveAccrualResetJob(
    LeaveDbContext dbContext,
    ILogger<LeaveAccrualResetJob> logger) : IRecurringJob
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var currentYear = DateTime.UtcNow.Year;
        var previousYear = currentYear - 1;

        var existingCurrentYearEmployees = await dbContext.LeaveBalances
            .IgnoreQueryFilters()
            .Where(b => b.Year == currentYear && !b.IsDeleted)
            .Select(b => b.EmployeeId)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var existingSet = existingCurrentYearEmployees.ToHashSet();

        var previousYearBalances = await dbContext.LeaveBalances
            .IgnoreQueryFilters()
            .Where(b => b.Year == previousYear && !b.IsDeleted)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var seededCount = 0;

        foreach (var balance in previousYearBalances)
        {
            if (existingSet.Contains(balance.EmployeeId)) continue;

            var newBalance = LeaveBalance.Seed(
                balance.TenantId,
                balance.EmployeeId,
                currentYear,
                balance.AnnualAllowance,
                balance.SickAllowance);

            await dbContext.LeaveBalances.AddAsync(newBalance, ct).ConfigureAwait(false);
            seededCount++;
        }

        if (seededCount > 0)
        {
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            logger.LogInformation(
                "Leave accrual reset: seeded {Count} leave balances for year {Year}",
                seededCount, currentYear);
        }
    }
}
