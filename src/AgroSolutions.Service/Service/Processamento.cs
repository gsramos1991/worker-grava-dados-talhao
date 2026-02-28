using AgroSolutions.Business.Interface;
using AgroSolutions.Domain.Models;
using AgroSolutions.TalhaoSensor.Api.DTOs;

namespace AgroSolutions.Service.Service
{
    public class Processamento
    {
        private readonly IDadosSensorService _service;

        public Processamento(IDadosSensorService service)
        {
            _service = service;
        }

        public async Task<bool> ProcessaDados(SensorCreateDto sensorDto)
        {
            var tratamentoDados = MapSensorDto(sensorDto);
            var status = await _service.ProcessarDadosSensor(tratamentoDados);
            return status;
        }

        private RegistroSensor MapSensorDto(SensorCreateDto sensor)
        {
            var sensorTratado = new RegistroSensor
            {
                TalhaoId = sensor.TalhaoId,
                DataHora = DateTime.Now,
                TemperaturaValor = sensor.Temperatura,
                UmidadeValor = sensor.Umidade,
                VelocidadeVento = (double)sensor.VelocidadeVento,
                StatusProcessamento = 1,
                EnumAlerta = GerarAlerta(sensor.Umidade),
                DataProcessamento = sensor.DataAfericao
            };

            return sensorTratado;
        }

        private string GerarAlerta(int UmidadeValor)
        {
            var alertas = new List<string>();

            if (UmidadeValor < 30)
            {
                return "Alerta de Seca";
            }

            if (UmidadeValor >= 70)
            {
                return "Risco de Praga";
            }

            return "Normal";
        }
    }
}
