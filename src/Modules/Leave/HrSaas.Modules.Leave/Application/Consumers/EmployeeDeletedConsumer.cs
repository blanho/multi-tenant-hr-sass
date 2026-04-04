using HrSaas.Contracts.Employee;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Domain.Entities;
using HrSaas.TenantSdk;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Leave.Application.Consumers;

public sealed class EmployeeDeletedConsumer(
    ILeaveRepository leaveRepository,
    TenantContext tenantContext,
    ILogger<EmployeeDeletedConsumer> logger) : IConsumer<EmployeeDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EmployeeDeletedIntegrationEvent> context)
    {
        var msg = context.Message;
        tenantContext.TenantId = msg.TenantId;

        var leaves = await leaveRepository
            .GetByEmployeeAsync(msg.TenantId, msg.EmployeeId, context.CancellationToken)
            .ConfigureAwait(false);

        var pendingLeaves = leaves
            .Where(l => l.Status == LeaveStatus.Pending)
            .ToList();

        if (pendingLeaves.Count == 0)
            return;

        foreach (var leave in pendingLeaves)
        {
            leave.Cancel(msg.EmployeeId);
            leaveRepository.Update(leave);
        }

        await leaveRepository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Cancelled {Count} pending leave requests for deleted employee {EmployeeId} in tenant {TenantId}",
            pendingLeaves.Count, msg.EmployeeId, msg.TenantId);
    }
}
