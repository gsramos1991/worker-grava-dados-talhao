using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgroSolutions.Infra.Ioc
{
    public static class AzureSecretsExtensions
    {
        public static IServiceCollection AddAzureSecrets(this IServiceCollection services, IConfiguration configuration)
        {
            var keyVaultUrl = configuration["KeyVault:Url"];

            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new InvalidOperationException(
                    "KeyVault:Url is not configured. Ensure it's set in appsettings.json under Values section.");
            }

            try
            {
                var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
                services.AddSingleton(client);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to initialize SecretClient with URL: {keyVaultUrl}", ex);
            }

            return services;
        }
    }
}
