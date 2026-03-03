using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public interface IRabbitMQService
    {
        Task PublishAsync(string queue, string message);
        Task<string> ConsumeAsync(string queue);
    }
}
