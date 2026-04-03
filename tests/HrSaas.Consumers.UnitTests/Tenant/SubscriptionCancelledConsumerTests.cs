using FluentAssertions;
using HrSaas.Contracts.Billing;
using HrSaas.Modules.Tenant.Application.Consumers;
using HrSaas.Modules.Tenant.Application.Interfaces;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using TenantEntity = HrSaas.Modules.Tenant.Domain.Entities.Tenant;

namespace HrSaas.Consumers.UnitTests.Tenant;

public sealed class SubscriptionCancelledConsumerTests : IAsyncDisposable
{
    private readonly ITenantRepository _repo = Substitute.For<ITenantRepository>();
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;

    public SubscriptionCancelledConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddScoped(_ => _repo)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<SubscriptionCancelledConsumer>();
                cfg.AddConsumer<SubscriptionPastDueConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task Cancelled_SuspendsTenant()
    {
        await _harness.Start();
        var tenant = TenantEntity.Create("Acme", "acme", "admin@acme.io");
        tenant.Activate();

        _repo.GetByIdAsync(tenant.TenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        await _harness.Bus.Publish(new SubscriptionCancelledIntegrationEvent(
            tenant.TenantId, Guid.NewGuid(), "Non-payment"));

        (await _harness.Consumed.Any<SubscriptionCancelledIntegrationEvent>()).Should().BeTrue();

        tenant.Status.Should().Be(HrSaas.Modules.Tenant.Domain.Entities.TenantStatus.Suspended);
        _repo.Received(1).Update(tenant);
    }

    [Fact]
    public async Task PastDue_SuspendsTenant()
    {
        await _harness.Start();
        var tenant = TenantEntity.Create("Acme", "acme", "admin@acme.io");
        tenant.Activate();

        _repo.GetByIdAsync(tenant.TenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        await _harness.Bus.Publish(new SubscriptionPastDueIntegrationEvent(
            tenant.TenantId, Guid.NewGuid()));

        (await _harness.Consumed.Any<SubscriptionPastDueIntegrationEvent>()).Should().BeTrue();

        tenant.Status.Should().Be(HrSaas.Modules.Tenant.Domain.Entities.TenantStatus.Suspended);
    }

    [Fact]
    public async Task Cancelled_TenantNotFound_Skips()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();

        _repo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns((TenantEntity?)null);

        await _harness.Bus.Publish(new SubscriptionCancelledIntegrationEvent(
            tenantId, Guid.NewGuid(), "Non-payment"));

        (await _harness.Consumed.Any<SubscriptionCancelledIntegrationEvent>()).Should().BeTrue();

        _repo.DidNotReceive().Update(Arg.Any<TenantEntity>());
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }
}
