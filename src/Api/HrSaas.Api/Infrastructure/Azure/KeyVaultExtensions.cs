using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace HrSaas.Api.Infrastructure.Azure;

public static class KeyVaultExtensions
{
    public static WebApplicationBuilder AddAzureKeyVault(this WebApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["Azure:KeyVault:VaultUri"];
        if (string.IsNullOrWhiteSpace(keyVaultUri))
            return builder;

        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());

        return builder;
    }
}
