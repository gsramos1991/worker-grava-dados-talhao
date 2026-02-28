using Azure.Messaging.ServiceBus;
using AgroSolutions.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgroSolutions.Infra.MessageQueue
{
    public class AzureServiceBusService : IMessageQueueService, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<AzureServiceBusService> _logger;

        public AzureServiceBusService(IConfiguration configuration, ILogger<AzureServiceBusService> logger)
        {
            _logger = logger;
            
            var connectionString = configuration["ConnectionStrings:AzureServiceBus"]
                ?? configuration.GetConnectionString("AzureServiceBus")
                ?? throw new ArgumentNullException(
                    "ConnectionString AzureServiceBus não encontrada. " +
                    "Certifique-se de que existe no Azure Key Vault com o nome 'ConnectionStrings--AzureServiceBus'");

            _client = new ServiceBusClient(connectionString);
            _logger.LogInformation("Azure Service Bus conectado com sucesso");
        }

        public async Task SendMessageAsync<T>(T message, string queueName) where T : class
        {
            try
            {
                var sender = _client.CreateSender(queueName);
                var messageJson = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(messageJson)
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                await sender.SendMessageAsync(serviceBusMessage);
                _logger.LogInformation("Mensagem enviada para a fila {QueueName} no Azure Service Bus", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem para a fila {QueueName} no Azure Service Bus", queueName);
                throw;
            }
        }

        public async Task SendMessageAsync(string message, string queueName)
        {
            try
            {
                var sender = _client.CreateSender(queueName);
                var serviceBusMessage = new ServiceBusMessage(message)
                {
                    ContentType = "text/plain",
                    MessageId = Guid.NewGuid().ToString()
                };

                await sender.SendMessageAsync(serviceBusMessage);
                _logger.LogInformation("Mensagem enviada para a fila {QueueName} no Azure Service Bus", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem para a fila {QueueName} no Azure Service Bus", queueName);
                throw;
            }
        }

        public Task<T?> ReceiveMessageAsync<T>(string queueName) where T : class
        {
            throw new NotImplementedException("Este serviço não consome mensagens, apenas envia");
        }

        public Task<string?> ReceiveMessageAsync(string queueName)
        {
            throw new NotImplementedException("Este serviço não consome mensagens, apenas envia");
        }

        public async ValueTask DisposeAsync()
        {
            if (_client != null)
            {
                await _client.DisposeAsync();
                _logger.LogInformation("Azure Service Bus desconectado");
            }
        }
    }
}
