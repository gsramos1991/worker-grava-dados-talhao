using AgroSolutions.Domain.Messages;
using AgroSolutions.Service.Service;
using AgroSolutions.TalhaoSensor.Api.DTOs;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Service.Consumers
{
    public class SensorConsumer : IConsumer<SensorMessage>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SensorConsumer> _logger;

        public SensorConsumer(IServiceScopeFactory scopeFactory, ILogger<SensorConsumer> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<SensorMessage> context)
        {
            await ProcessMessageAsync(context.Message, context.CancellationToken);
        }

        public async Task ProcessMessageAsync(SensorMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[CONSUMER] Mensagem recebida da fila. TalhaoId: {TalhaoId} | Umidade: {Umidade} | Temperatura: {Temperatura} | IndiceUv: {IndiceUv} | VelocidadeVento: {VelocidadeVento} | DataAfericao: {DataAfericao}",
                message.TalhaoId, message.Umidade, message.Temperatura,
                message.IndiceUv, message.VelocidadeVento, message.DataAfericao);

            _logger.LogInformation(
                "[CONSUMER] Iniciando processamento — TalhaoId: {TalhaoId} | DataAfericao: {DataAfericao}",
                message.TalhaoId,
                message.DataAfericao);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var processamento = scope.ServiceProvider.GetRequiredService<Processamento>();

            var dto = new SensorCreateDto
            {
                TalhaoId = message.TalhaoId,
                Umidade = message.Umidade,
                DataAfericao = message.DataAfericao,
                Temperatura = message.Temperatura,
                IndiceUv = message.IndiceUv,
                VelocidadeVento = message.VelocidadeVento
            };

            var resultado = await processamento.ProcessaDados(dto);

            if (resultado)
                _logger.LogInformation(
                    "Dados do sensor para o TalhaoId {TalhaoId} processados com sucesso.",
                    message.TalhaoId);
            else
                _logger.LogWarning(
                    "Processamento dos dados do sensor para o TalhaoId {TalhaoId} retornou falha sem exceção.",
                    message.TalhaoId);
        }
    }
}
