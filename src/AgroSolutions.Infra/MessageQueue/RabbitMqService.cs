using AgroSolutions.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgroSolutions.Infra.MessageQueue
{
    public class RabbitMqService : IMessageQueueService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
        {
            _logger = logger;
            
            // Busca a connection string do Azure Key Vault via IConfiguration
            // No Key Vault, o secret deve ser nomeado como "ConnectionStrings--RabbitMQ"
            var connectionString = configuration["ConnectionStrings:RabbitMQ"]
                ?? configuration.GetConnectionString("RabbitMQ")
                ?? throw new ArgumentNullException(
                    "ConnectionString RabbitMQ não encontrada. " +
                    "Certifique-se de que existe no Azure Key Vault com o nome 'ConnectionStrings--RabbitMQ'");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _logger.LogInformation("RabbitMQ conectado com sucesso");
        }

        public async Task SendMessageAsync<T>(T message, string queueName) where T : class
        {
            try
            {
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Mensagem enviada para a fila {QueueName} no RabbitMQ", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem para a fila {QueueName} no RabbitMQ", queueName);
                throw;
            }
        }

        public async Task SendMessageAsync(string message, string queueName)
        {
            try
            {
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "text/plain",
                    MessageId = Guid.NewGuid().ToString()
                };

                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Mensagem enviada para a fila {QueueName} no RabbitMQ", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem para a fila {QueueName} no RabbitMQ", queueName);
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

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ desconectado");
        }
    }
}
