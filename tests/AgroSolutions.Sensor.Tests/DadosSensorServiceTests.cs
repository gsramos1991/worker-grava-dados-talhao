using AgroSolutions.Business.Services;
using AgroSolutions.Domain.Interfaces;
using AgroSolutions.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace AgroSolutions.Sensor.Tests
{
    public class DadosSensorServiceTests
    {

        [Fact]
        public async Task ProcessarDadosSensor_FalhaRepositorio_RetornaFalseSemEnviarFila()
        {
            var registro = CriarRegistroValido();
            var repository = new FakeDataAccessRepository { Resultado = false };
            var queue = new FakeMessageQueueService();
            var service = new DadosSensorService(repository, queue, NullLogger<DadosSensorService>.Instance);

            var resultado = await service.ProcessarDadosSensor(registro);

            Assert.False(resultado);
            Assert.True(repository.FoiChamado);
            Assert.Null(queue.UltimaMensagem);
            Assert.Null(queue.UltimaFila);
        }

        [Fact]
        public async Task ProcessarDadosSensor_RegistroNulo_DisparaExcecao()
        {
            var repository = new FakeDataAccessRepository { Resultado = true };
            var queue = new FakeMessageQueueService();
            var service = new DadosSensorService(repository, queue, NullLogger<DadosSensorService>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.ProcessarDadosSensor(null!));

            Assert.False(repository.FoiChamado);
            Assert.Null(queue.UltimaMensagem);
        }

        [Theory]
        [InlineData(-1, 1)]
        [InlineData(1, -1)]
        public async Task ProcessarDadosSensor_ValoresInvalidos_DisparaExcecao(int umidade, double vento)
        {
            var registro = CriarRegistroValido();
            registro.UmidadeValor = umidade;
            registro.VelocidadeVento = vento;

            var repository = new FakeDataAccessRepository { Resultado = true };
            var queue = new FakeMessageQueueService();
            var service = new DadosSensorService(repository, queue, NullLogger<DadosSensorService>.Instance);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.ProcessarDadosSensor(registro));

            Assert.False(repository.FoiChamado);
            Assert.Null(queue.UltimaMensagem);
        }

        [Fact]
        public async Task ProcessarDadosSensor_ErroRepositorio_DisparaInvalidOperationException()
        {
            var registro = CriarRegistroValido();
            var repository = new FakeDataAccessRepository { Excecao = new Exception("falha") };
            var queue = new FakeMessageQueueService();
            var service = new DadosSensorService(repository, queue, NullLogger<DadosSensorService>.Instance);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessarDadosSensor(registro));

            Assert.Equal("Erro ao processar dados do sensor.", ex.Message);
            Assert.Null(queue.UltimaMensagem);
        }

        private static RegistroSensor CriarRegistroValido()
        {
            return new RegistroSensor
            {
                TalhaoId = Guid.NewGuid(),
                DataHora = DateTime.UtcNow,
                TemperaturaValor = 25.5,
                UmidadeValor = 10,
                VelocidadeVento = 5.5,
                StatusProcessamento = 1,
                EnumAlerta = "ok",
                DataProcessamento = DateTime.UtcNow
            };
        }
    }

    internal sealed class FakeDataAccessRepository : IDataAccessRepository
    {
        public bool Resultado { get; set; }
        public bool FoiChamado { get; private set; }
        public Exception? Excecao { get; set; }

        public Task<bool> GravarDadosSensor(RegistroSensor registro)
        {
            FoiChamado = true;
            if (Excecao is not null)
            {
                throw Excecao;
            }

            return Task.FromResult(Resultado);
        }
    }

    internal sealed class FakeMessageQueueService : IMessageQueueService
    {
        public string? UltimaMensagem { get; private set; }
        public string? UltimaFila { get; private set; }

        public Task SendMessageAsync<T>(T message, string queueName) where T : class
        {
            UltimaMensagem = JsonSerializer.Serialize(message);
            UltimaFila = queueName;
            return Task.CompletedTask;
        }

        public Task<T?> ReceiveMessageAsync<T>(string queueName) where T : class
        {
            return Task.FromResult<T?>(null);
        }

        public Task SendMessageAsync(string message, string queueName)
        {
            UltimaMensagem = message;
            UltimaFila = queueName;
            return Task.CompletedTask;
        }

        public Task<string?> ReceiveMessageAsync(string queueName)
        {
            return Task.FromResult<string?>(null);
        }
    }
}
