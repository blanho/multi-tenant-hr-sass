using FluentAssertions;
using HrSaas.Contracts.Tenant;
using HrSaas.Modules.Identity.Application.Consumers;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.Modules.Identity.Domain.ValueObjects;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HrSaas.Consumers.UnitTests.Identity;

public sealed class TenantSuspendedConsumerTests : IAsyncDisposable
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;

    public TenantSuspendedConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddScoped(_ => _repo)
            .AddScoped<HrSaas.TenantSdk.TenantContext>()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<TenantSuspendedConsumer>();
                cfg.AddConsumer<TenantReactivatedConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task TenantSuspended_DeactivatesAllUsers()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();

        var user1 = AppUser.Create(tenantId, Email.Create("a@test.io"), HashedPassword.FromHash("hash"), Guid.NewGuid());
        var user2 = AppUser.Create(tenantId, Email.Create("b@test.io"), HashedPassword.FromHash("hash"), Guid.NewGuid());

        _repo.GetAllAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(new List<AppUser> { user1, user2 });

        await _harness.Bus.Publish(new TenantSuspendedIntegrationEvent(tenantId, "Non-payment"));

        (await _harness.Consumed.Any<TenantSuspendedIntegrationEvent>()).Should().BeTrue();

        user1.IsActive.Should().BeFalse();
        user2.IsActive.Should().BeFalse();
        _repo.Received(2).Update(Arg.Any<AppUser>());
    }

    [Fact]
    public async Task TenantReactivated_ReactivatesUsers()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();

        var user = AppUser.Create(tenantId, Email.Create("a@test.io"), HashedPassword.FromHash("hash"), Guid.NewGuid());
        user.Deactivate();

        _repo.GetAllAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(new List<AppUser> { user });

        await _harness.Bus.Publish(new TenantReactivatedIntegrationEvent(tenantId));

        (await _harness.Consumed.Any<TenantReactivatedIntegrationEvent>()).Should().BeTrue();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task TenantSuspended_NoUsers_Skips()
    {
        await _harness.Start();
        var tenantId = Guid.NewGuid();

        _repo.GetAllAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(new List<AppUser>());

        await _harness.Bus.Publish(new TenantSuspendedIntegrationEvent(tenantId, "Non-payment"));

        (await _harness.Consumed.Any<TenantSuspendedIntegrationEvent>()).Should().BeTrue();

        _repo.DidNotReceive().Update(Arg.Any<AppUser>());
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }
}
