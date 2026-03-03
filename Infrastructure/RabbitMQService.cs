using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly string _connectionString;

        public RabbitMQService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task PublishAsync(string queue, string message)
        {
            var factory = new ConnectionFactory() { Uri = new System.Uri(_connectionString) };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: queue, body: body);
            await Task.CompletedTask;
        }

        public async Task<string> ConsumeAsync(string queue)
        {
            var factory = new ConnectionFactory() { Uri = new System.Uri(_connectionString) };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            var consumer = new EventingBasicConsumer(channel);
            string result = null;
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                result = Encoding.UTF8.GetString(body);
            };
            channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
            await Task.Delay(500); // Wait for message
            return result;
        }
    }
}
