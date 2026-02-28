using AgroSolutions.Business.Interface;
using AgroSolutions.Business.Services;
using AgroSolutions.Domain.Interfaces;
using AgroSolutions.Infra.Data.Repository;
using AgroSolutions.Infra.Ioc;
using AgroSolutions.Infra.MessageQueue;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgroSolutions.Sensor.Tests
{
    public class IocExtensionsTests
    {
        [Fact]
        public void AddRegisterServices_RegistraServico()
        {
            var services = new ServiceCollection();

            services.AddRegisterServices();

            var descriptor = Assert.Single(services, item => item.ServiceType == typeof(IDadosSensorService));
            Assert.Equal(typeof(DadosSensorService), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void AddRegisterRepositories_RegistraRepositorio()
        {
            var services = new ServiceCollection();

            services.AddRegisterRepositories();

            var descriptor = Assert.Single(services, item => item.ServiceType == typeof(IDataAccessRepository));
            Assert.Equal(typeof(DataAccessRepository), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Theory]
        [InlineData("AzureServiceBus", typeof(AzureServiceBusService))]
        [InlineData("RabbitMQ", typeof(RabbitMqService))]
        public void AddFilaQueue_RegistraProvider(string provider, Type implementationType)
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MessageQueue:Provider"] = provider
                })
                .Build();

            services.AddFilaQueue(configuration);

            var descriptor = Assert.Single(services, item => item.ServiceType == typeof(IMessageQueueService));
            Assert.Equal(implementationType, descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void AddFilaQueue_ProviderInvalido_DisparaExcecao()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MessageQueue:Provider"] = "invalido"
                })
                .Build();

            Assert.Throws<InvalidOperationException>(() => services.AddFilaQueue(configuration));
        }

        [Fact]
        public void AddAzureSecrets_SemUrl_DisparaExcecao()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            Assert.Throws<InvalidOperationException>(() => services.AddAzureSecrets(configuration));
        }

        [Fact]
        public void AddAzureSecrets_ComUrl_RegistraSecretClient()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["KeyVault:Url"] = "https://example.vault.azure.net/"
                })
                .Build();

            services.AddAzureSecrets(configuration);

            var descriptor = Assert.Single(services, item => item.ServiceType == typeof(SecretClient));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void AddSwaggerDocumentation_AdicionaServicos()
        {
            var services = new ServiceCollection();
            var antes = services.Count;

            services.AddSwaggerDocumentation();

            Assert.True(services.Count > antes);
        }

        [Fact]
        public void AddOpenTelemetryApp_AdicionaServicos()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var antes = services.Count;

            services.AddOpenTelemetryApp(configuration);

            Assert.True(services.Count > antes);
        }

        [Fact]
        public void AddSerilogApp_RetornaMesmaColecao()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            var retorno = services.AddSerilogApp(configuration);

            Assert.Same(services, retorno);
        }
    }
}
