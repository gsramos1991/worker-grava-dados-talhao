using AgroSolutions.Infra.Data.Repository;
using AgroSolutions.Infra.MessageQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgroSolutions.Sensor.Tests
{
    public class InfraServicesTests
    {
        [Fact]
        public void DataAccessRepository_SemConnectionString_DisparaExcecao()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            Assert.Throws<InvalidOperationException>(() => new DataAccessRepository(configuration));
        }

        [Fact]
        public void AzureServiceBusService_SemConnectionString_DisparaExcecao()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            Assert.Throws<ArgumentNullException>(() => new AzureServiceBusService(configuration, NullLogger<AzureServiceBusService>.Instance));
        }

        [Fact]
        public void RabbitMqService_SemConnectionString_DisparaExcecao()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            Assert.Throws<ArgumentNullException>(() => new RabbitMqService(configuration, NullLogger<RabbitMqService>.Instance));
        }
    }
}
