using EPG.Samples.API.Extensions;
using EPG.Samples.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EPG.Samples.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly RabbitMqConfig _rabbitMqConfig;
        private readonly ConnectionFactory _factory;

        public MessagesController(IOptions<RabbitMqConfig> rabbitMqConfig)
        {
            _rabbitMqConfig = rabbitMqConfig.Value;

            _factory = new ConnectionFactory
            {
                HostName = _rabbitMqConfig.Host
            };
        }

        [HttpPost]
        public IActionResult PostMessage([FromBody] PostMessageRequest postMessageRequest)
        {
            var messageSend = new MessageBusSend
            {
                Content = postMessageRequest.Content
            };

            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: _rabbitMqConfig.Queue,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                        );

                    var stringfiedMessage = JsonSerializer.Serialize(messageSend);
                    var bytesMessage = Encoding.UTF8.GetBytes(stringfiedMessage);

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: _rabbitMqConfig.Queue,
                        mandatory: false,
                        basicProperties: null,
                        body: bytesMessage
                        );
                }
            }

            return Accepted();
        }
    }
}
