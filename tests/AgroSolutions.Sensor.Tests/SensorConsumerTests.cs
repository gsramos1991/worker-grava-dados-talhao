using AgroSolutions.Business.Interface;
using AgroSolutions.Domain.Messages;
using AgroSolutions.Domain.Models;
using AgroSolutions.Service.Consumers;
using AgroSolutions.Service.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgroSolutions.Sensor.Tests
{
    public class SensorConsumerTests
    {
        [Fact]
        public async Task ProcessMessageAsync_Sucesso_ChamaProcessaDados()
        {
            // Arrange
            var fakeService = new FakeSensorDadosSensorService { Resultado = true };
            var provider = BuildProvider(fakeService);
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var consumer = new SensorConsumer(scopeFactory, NullLogger<SensorConsumer>.Instance);

            var message = new SensorMessage
            {
                TalhaoId = Guid.NewGuid(),
                Umidade = 60,
                DataAfericao = DateTime.UtcNow,
                Temperatura = 25.5,
                IndiceUv = 3,
                VelocidadeVento = 5.2m
            };

            // Act
            await consumer.ProcessMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.True(fakeService.FoiChamado);
            Assert.Equal(message.TalhaoId, fakeService.UltimoRegistro!.TalhaoId);
        }

        [Fact]
        public async Task ProcessMessageAsync_ServicoLancaExcecao_PropagaExcecao()
        {
            // Arrange
            var fakeService = new FakeSensorDadosSensorService { DeveJogar = true };
            var provider = BuildProvider(fakeService);
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var consumer = new SensorConsumer(scopeFactory, NullLogger<SensorConsumer>.Instance);

            var message = new SensorMessage { TalhaoId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                consumer.ProcessMessageAsync(message, CancellationToken.None));
        }

        [Fact]
        public void Constructor_ScopeFactoryNulo_LancaArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SensorConsumer(null!, NullLogger<SensorConsumer>.Instance));
        }

        [Fact]
        public void Constructor_LoggerNulo_LancaArgumentNullException()
        {
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            Assert.Throws<ArgumentNullException>(() =>
                new SensorConsumer(provider.GetRequiredService<IServiceScopeFactory>(), null!));
        }

        private static ServiceProvider BuildProvider(IDadosSensorService service)
        {
            var services = new ServiceCollection();
            services.AddScoped<IDadosSensorService>(_ => service);
            services.AddScoped<Processamento>();
            return services.BuildServiceProvider();
        }
    }

    internal sealed class FakeSensorDadosSensorService : IDadosSensorService
    {
        public bool Resultado { get; set; } = true;
        public bool DeveJogar { get; set; }
        public bool FoiChamado { get; private set; }
        public RegistroSensor? UltimoRegistro { get; private set; }

        public Task<bool> ProcessarDadosSensor(RegistroSensor registro)
        {
            FoiChamado = true;
            UltimoRegistro = registro;
            if (DeveJogar) throw new InvalidOperationException("Falha simulada no processamento.");
            return Task.FromResult(Resultado);
        }
    }
}
