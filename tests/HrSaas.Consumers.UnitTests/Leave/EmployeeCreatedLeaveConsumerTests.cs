using FluentAssertions;
using HrSaas.Contracts.Employee;
using HrSaas.Modules.Leave.Application.Consumers;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Application.Policies;
using HrSaas.Modules.Leave.Domain.Entities;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HrSaas.Consumers.UnitTests.Leave;

public sealed class EmployeeCreatedLeaveConsumerTests : IAsyncDisposable
{
    private readonly ILeaveBalanceRepository _repo = Substitute.For<ILeaveBalanceRepository>();
    private readonly ILeaveBalancePolicy _policy = Substitute.For<ILeaveBalancePolicy>();
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;

    public EmployeeCreatedLeaveConsumerTests()
    {
        _policy.GetAnnualAllowance(Arg.Any<int>()).Returns(20);
        _policy.GetSickAllowance(Arg.Any<int>()).Returns(10);

        _provider = new ServiceCollection()
            .AddScoped(_ => _repo)
            .AddScoped(_ => _policy)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<EmployeeCreatedConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task EmployeeCreated_SeedsLeaveBalance()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _repo.GetAsync(tenantId, employeeId, DateTime.UtcNow.Year, Arg.Any<CancellationToken>())
            .Returns((LeaveBalance?)null);

        await _harness.Bus.Publish(new EmployeeCreatedIntegrationEvent(
            tenantId, employeeId, "Jane", "HR", "Manager", "jane@acme.io"));

        (await _harness.Consumed.Any<EmployeeCreatedIntegrationEvent>()).Should().BeTrue();

        await _repo.Received(1).AddAsync(
            Arg.Is<LeaveBalance>(b =>
                b.TenantId == tenantId &&
                b.EmployeeId == employeeId &&
                b.AnnualAllowance == 20 &&
                b.SickAllowance == 10),
            Arg.Any<CancellationToken>());

        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EmployeeCreated_ExistingBalance_Skips()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _repo.GetAsync(tenantId, employeeId, DateTime.UtcNow.Year, Arg.Any<CancellationToken>())
            .Returns(LeaveBalance.Seed(tenantId, employeeId, DateTime.UtcNow.Year));

        await _harness.Bus.Publish(new EmployeeCreatedIntegrationEvent(
            tenantId, employeeId, "Jane", "HR", "Manager", "jane@acme.io"));

        (await _harness.Consumed.Any<EmployeeCreatedIntegrationEvent>()).Should().BeTrue();

        await _repo.DidNotReceive().AddAsync(
            Arg.Any<LeaveBalance>(), Arg.Any<CancellationToken>());
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }
}
