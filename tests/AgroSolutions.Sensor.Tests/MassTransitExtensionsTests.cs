using AgroSolutions.Infra.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgroSolutions.Sensor.Tests
{
    public class MassTransitExtensionsTests
    {
        [Fact]
        public void AddMassTransitQueue_ConnectionStringAusente_LancaInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                services.AddMassTransitQueue(configuration, _ => { }));

            Assert.Contains("ConnectionStrings:AzureServiceBus", ex.Message);
        }

        [Fact]
        public void AddMassTransitQueue_InputQueueNameAusente_LancaInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:AzureServiceBus"] = "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=fakekey"
                })
                .Build();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                services.AddMassTransitQueue(configuration, _ => { }));

            Assert.Contains("MessageQueue:InputQueueName", ex.Message);
        }
    }
}
