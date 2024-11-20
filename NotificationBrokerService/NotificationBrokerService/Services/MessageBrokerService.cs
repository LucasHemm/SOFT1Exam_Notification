// Services/MessageBrokerService.cs
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using NotificationBrokerService.Models;
using NotificationBrokerService.Services;

namespace RabbitMQBrokerService.Services
{
    public class MessageBrokerService : IMessageBrokerService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMQSettings _settings;

        public MessageBrokerService(IOptions<RabbitMQSettings> options)
        {
            _settings = options.Value;

            var factory = new ConnectionFactory()
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Publish(string exchange, string routingKey, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);

            _channel.BasicPublish(exchange: exchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}