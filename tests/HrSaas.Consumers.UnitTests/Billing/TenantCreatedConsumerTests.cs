using FluentAssertions;
using HrSaas.Contracts.Tenant;
using HrSaas.Modules.Billing.Application.Consumers;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Application.Policies;
using HrSaas.Modules.Billing.Domain.Entities;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HrSaas.Consumers.UnitTests.Billing;

public sealed class TenantCreatedConsumerTests : IAsyncDisposable
{
    private readonly ISubscriptionRepository _repo = Substitute.For<ISubscriptionRepository>();
    private readonly IBillingPolicy _policy = Substitute.For<IBillingPolicy>();
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;

    public TenantCreatedConsumerTests()
    {
        _policy.GetDefaultTrialPlan().Returns("Professional");
        _policy.GetTrialDays("Professional").Returns(30);

        _provider = new ServiceCollection()
            .AddScoped(_ => _repo)
            .AddScoped(_ => _policy)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<TenantCreatedConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task Handle_NewTenant_CreatesTrialSubscription()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();

        _repo.GetActiveByTenantAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        await _harness.Bus.Publish(new TenantCreatedIntegrationEvent(
            tenantId, "Acme Corp", "acme-corp", "admin@acme.io", "Professional"));

        (await _harness.Consumed.Any<TenantCreatedIntegrationEvent>()).Should().BeTrue();

        await _repo.Received(1).AddAsync(
            Arg.Is<Subscription>(s => s.TenantId == tenantId),
            Arg.Any<CancellationToken>());

        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingSubscription_Skips()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();

        _repo.GetActiveByTenantAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(Subscription.CreateFree(tenantId));

        await _harness.Bus.Publish(new TenantCreatedIntegrationEvent(
            tenantId, "Acme Corp", "acme-corp", "admin@acme.io", "Professional"));

        (await _harness.Consumed.Any<TenantCreatedIntegrationEvent>()).Should().BeTrue();

        await _repo.DidNotReceive().AddAsync(
            Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }
}
