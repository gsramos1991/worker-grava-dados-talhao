using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgroSolutions.Infra.Ioc
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddMassTransitQueue(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator> configureConsumers)
        {
            var connectionString = configuration["ConnectionStrings:AzureServiceBus"]
                ?? configuration.GetConnectionString("AzureServiceBus");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "A connection string 'ConnectionStrings:AzureServiceBus' não foi encontrada. " +
                    "Verifique a configuração do Azure Key Vault (segredo: ConnectionStrings--AzureServiceBus).");

            var queueName = configuration["MessageQueue:InputQueueName"];

            if (string.IsNullOrWhiteSpace(queueName))
                throw new InvalidOperationException(
                    "A chave de configuração 'MessageQueue:InputQueueName' não foi encontrada.");

            services.AddMassTransit(x =>
            {
                configureConsumers(x);

                x.UsingAzureServiceBus((ctx, cfg) =>
                {
                    cfg.Host(connectionString);

                    cfg.ReceiveEndpoint(queueName, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.UseRawJsonDeserializer();
                        e.UseMessageRetry(r => r.Immediate(3));
                        e.ConfigureConsumers(ctx);
                    });
                });
            });

            return services;
        }
    }
}
