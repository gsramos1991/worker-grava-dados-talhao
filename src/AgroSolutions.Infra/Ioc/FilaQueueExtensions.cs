using AgroSolutions.Domain.Interfaces;
using AgroSolutions.Infra.MessageQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AgroSolutions.Infra.Ioc
{
    public static class FilaQueueExtensions
    {
        public static IServiceCollection AddFilaQueue(this IServiceCollection services, IConfiguration configuration)
        {
            var queueProvider = configuration["MessageQueue:Provider"]?.ToLower() ?? "rabbitmq";

            switch (queueProvider)
            {
                case "azureservicebus":
                case "azure":
                    services.AddSingleton<IMessageQueueService, AzureServiceBusService>();
                    break;

                case "rabbitmq":
                case "rabbit":
                    services.AddSingleton<IMessageQueueService, RabbitMqService>();
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Provider de fila '{queueProvider}' não é suportado. " +
                        "Use 'AzureServiceBus' ou 'RabbitMQ'");
            }

            return services;
        }
    }
}
