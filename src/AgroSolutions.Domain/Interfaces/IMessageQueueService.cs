using System.Threading.Tasks;

namespace AgroSolutions.Domain.Interfaces
{
    public interface IMessageQueueService
    {
        Task SendMessageAsync<T>(T message, string queueName) where T : class;
        Task<T?> ReceiveMessageAsync<T>(string queueName) where T : class;
        Task SendMessageAsync(string message, string queueName);
        Task<string?> ReceiveMessageAsync(string queueName);
    }
}
