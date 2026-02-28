using AgroSolutions.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AgroSolutions.Examples
{
    /// <summary>
    /// Exemplo de uso do serviço de fila de mensagens
    /// </summary>
    public class MessageQueueUsageExample
    {
        private readonly IMessageQueueService _messageQueue;
        private readonly ILogger<MessageQueueUsageExample> _logger;

        public MessageQueueUsageExample(
            IMessageQueueService messageQueue,
            ILogger<MessageQueueUsageExample> logger)
        {
            _messageQueue = messageQueue;
            _logger = logger;
        }

        /// <summary>
        /// Exemplo de uso em um controller ou service
        /// Importante: Use o DTO correto do seu projeto (ex: SensorCreateDto)
        /// </summary>
        public async Task ProcessarEEnviarParaFila<T>(T dados, string queueName) where T : class
        {
            try
            {
                _logger.LogInformation("Processando dados para envio à fila {QueueName}", queueName);

                // Salvar no banco de dados (exemplo)
                // await _repository.SaveAsync(dados);

                // Enviar para fila para processamento assíncrono
                await _messageQueue.SendMessageAsync(dados, queueName);

                _logger.LogInformation("Dados salvos e enviados para processamento assíncrono");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar dados");
                throw;
            }
        }
    }
}
