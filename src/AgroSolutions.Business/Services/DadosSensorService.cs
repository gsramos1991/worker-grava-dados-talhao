using AgroSolutions.Business.Interface;
using AgroSolutions.Domain.Diagnostics;
using AgroSolutions.Domain.Interfaces;
using AgroSolutions.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgroSolutions.Business.Services
{
    public class DadosSensorService : IDadosSensorService
    {
        private readonly IDataAccessRepository _repository;
        private readonly IMessageQueueService _queue;
        private readonly ILogger<DadosSensorService> _logger;

        public DadosSensorService(IDataAccessRepository repository, IMessageQueueService queue, ILogger<DadosSensorService> logger)
        {
            _repository = repository;
            _queue = queue;
            _logger = logger;
        }

        public async Task<bool> ProcessarDadosSensor(RegistroSensor registro)
        {
            using var activity = AgroSolutionsDiagnostics.Source.StartActivity(
                "sensor.processar", ActivityKind.Internal);

            activity?.SetTag("talhao.id", registro.TalhaoId);
            activity?.SetTag("sensor.temperatura", registro.TemperaturaValor);
            activity?.SetTag("sensor.umidade", registro.UmidadeValor);
            activity?.SetTag("sensor.velocidade_vento", registro.VelocidadeVento);
            activity?.SetTag("sensor.data_afericao", registro.DataProcessamento?.ToString("o"));

            try
            {
                _logger.LogInformation("Validar dados obrigatorios do request");
                ArgumentNullException.ThrowIfNull(registro, nameof(registro));
                ArgumentOutOfRangeException.ThrowIfLessThan(registro.UmidadeValor, 0, nameof(registro.UmidadeValor));
                ArgumentOutOfRangeException.ThrowIfLessThan(registro.VelocidadeVento, 0, nameof(registro.VelocidadeVento));
                ArgumentNullException.ThrowIfNull(registro.DataProcessamento, nameof(registro.DataProcessamento));

                var resultado = await _repository.GravarDadosSensor(registro);

                if (resultado)
                {
                    _logger.LogInformation("Dados do sensor gravados com sucesso no banco de dados.");
                    activity?.SetTag("sensor.gravado", true);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return true;
                }

                _logger.LogWarning("Falha ao gravar dados do sensor no banco de dados.");
                activity?.SetTag("sensor.gravado", false);
                activity?.SetStatus(ActivityStatusCode.Error, "Falha ao gravar no banco");

                return resultado;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Dados de entrada inválidos para o registro do sensor.");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", nameof(ArgumentException));
                activity?.SetTag("exception.message", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar dados do sensor.");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                activity?.SetTag("exception.message", ex.Message);
                throw new InvalidOperationException("Erro ao processar dados do sensor.", ex);
            }
        }
    }
}
