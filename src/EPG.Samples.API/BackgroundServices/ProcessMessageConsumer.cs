using EPG.Samples.API.Extensions;
using EPG.Samples.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EPG.Samples.API.BackgroundServices
{
    public class ProcessMessageConsumer : BackgroundService
    {
        private readonly RabbitMqConfig _rabbitMqConfig;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public ProcessMessageConsumer(IOptions<RabbitMqConfig> rabbitMqConfig)
        {
            _rabbitMqConfig = rabbitMqConfig.Value;

            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqConfig.Host
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _rabbitMqConfig.Queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonSerializer.Deserialize<MessageBusSend>(contentString);

                // Processar a mensagem recebida

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(_rabbitMqConfig.Queue, false, consumer);

            return Task.CompletedTask;
        }
    }
}
