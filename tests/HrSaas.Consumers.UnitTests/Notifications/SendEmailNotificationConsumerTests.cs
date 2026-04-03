using FluentAssertions;
using HrSaas.Contracts.Notifications;
using HrSaas.Modules.Notifications.Application.Consumers;
using HrSaas.Modules.Notifications.Application.Interfaces;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HrSaas.Consumers.UnitTests.Notifications;

public sealed class SendEmailNotificationConsumerTests : IAsyncDisposable
{
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;

    public SendEmailNotificationConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddScoped(_ => _notificationService)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<SendEmailNotificationConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task SendEmail_DelegatesTo_INotificationService()
    {
        await _harness.Start();

        var cmd = new SendEmailNotificationCommand
        {
            TenantId = Guid.NewGuid(),
            ToEmail = "user@test.io",
            Subject = "Welcome",
            BodyHtml = "<h1>Welcome</h1>",
            BodyText = "Welcome"
        };

        await _harness.Bus.Publish(cmd);

        (await _harness.Consumed.Any<SendEmailNotificationCommand>()).Should().BeTrue();

        await _notificationService.Received(1).SendEmailAsync(
            "user@test.io",
            "Welcome",
            "<h1>Welcome</h1>",
            "Welcome",
            Arg.Any<CancellationToken>());
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }
}
