using FluentAssertions;
using HrSaas.Contracts.Employee;
using HrSaas.Modules.Billing.Application.Consumers;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Domain.Entities;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HrSaas.Consumers.UnitTests.Billing;

public sealed class EmployeeSeatConsumerTests : IAsyncDisposable
{
    private readonly ISubscriptionRepository _repo = Substitute.For<ISubscriptionRepository>();
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;

    public EmployeeSeatConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddScoped(_ => _repo)
            .AddScoped<HrSaas.TenantSdk.TenantContext>()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<EmployeeCreatedConsumer>();
                cfg.AddConsumer<EmployeeDeletedConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task EmployeeCreated_IncrementsSeats()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();
        var sub = Subscription.CreateTrial(tenantId, "Professional");

        _repo.GetActiveByTenantAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(sub);

        var initialSeats = sub.UsedSeats;

        await _harness.Bus.Publish(new EmployeeCreatedIntegrationEvent(
            tenantId, Guid.NewGuid(), "John", "Engineering", "Dev", "john@acme.io"));

        (await _harness.Consumed.Any<EmployeeCreatedIntegrationEvent>()).Should().BeTrue();

        sub.UsedSeats.Should().Be(initialSeats + 1);
        _repo.Received(1).Update(sub);
    }

    [Fact]
    public async Task EmployeeDeleted_DecrementsSeats()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();
        var sub = Subscription.CreateTrial(tenantId, "Professional");
        sub.IncrementSeats();

        _repo.GetActiveByTenantAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(sub);

        await _harness.Bus.Publish(new EmployeeDeletedIntegrationEvent(
            tenantId, Guid.NewGuid()));

        (await _harness.Consumed.Any<EmployeeDeletedIntegrationEvent>()).Should().BeTrue();

        sub.UsedSeats.Should().Be(0);
    }

    [Fact]
    public async Task EmployeeCreated_NoSubscription_Skips()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();

        _repo.GetActiveByTenantAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        await _harness.Bus.Publish(new EmployeeCreatedIntegrationEvent(
            tenantId, Guid.NewGuid(), "John", "Engineering", "Dev", "john@acme.io"));

        (await _harness.Consumed.Any<EmployeeCreatedIntegrationEvent>()).Should().BeTrue();

        _repo.DidNotReceive().Update(Arg.Any<Subscription>());
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }
}
