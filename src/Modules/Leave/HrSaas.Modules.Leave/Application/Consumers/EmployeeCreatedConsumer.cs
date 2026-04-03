using HrSaas.Contracts.Employee;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Application.Policies;
using HrSaas.Modules.Leave.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Leave.Application.Consumers;

public sealed class EmployeeCreatedConsumer(
    ILeaveBalanceRepository leaveBalanceRepository,
    ILeaveBalancePolicy policy,
    ILogger<EmployeeCreatedConsumer> logger) : IConsumer<EmployeeCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EmployeeCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        var currentYear = DateTime.UtcNow.Year;

        var existing = await leaveBalanceRepository
            .GetAsync(msg.TenantId, msg.EmployeeId, currentYear, context.CancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return;
        }

        var balance = LeaveBalance.Seed(
            msg.TenantId,
            msg.EmployeeId,
            currentYear,
            policy.GetAnnualAllowance(currentYear),
            policy.GetSickAllowance(currentYear));

        await leaveBalanceRepository.AddAsync(balance, context.CancellationToken).ConfigureAwait(false);
        await leaveBalanceRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Seeded leave balance for employee {EmployeeId} in tenant {TenantId} for year {Year}.",
            msg.EmployeeId, msg.TenantId, currentYear);
    }
}
