using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.IntegrationTests.Infrastructure;

public sealed class HrSaasWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.Configure<Microsoft.Extensions.Hosting.HostOptions>(opts =>
                opts.BackgroundServiceExceptionBehavior = Microsoft.Extensions.Hosting.BackgroundServiceExceptionBehavior.Ignore);
        });

        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.ConnectionString);
    }

    public async Task InitializeAsync() => await _postgres.InitializeAsync();
    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
