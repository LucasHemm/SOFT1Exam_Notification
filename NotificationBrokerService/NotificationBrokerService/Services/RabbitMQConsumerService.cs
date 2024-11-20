// Services/NotificationConsumerService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NotificationBrokerService.Models;

namespace NotificationService.Services
{
    public class NotificationConsumerService : BackgroundService
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<NotificationConsumerService> _logger;
        private IConnection _connection;
        private IModel _channel;

        public NotificationConsumerService(IOptions<RabbitMQSettings> options, ILogger<NotificationConsumerService> logger)
        {
            _settings = options.Value;
            _logger = logger;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
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

            // Declare the exchange and queue
            _channel.ExchangeDeclare(exchange: "user_exchange", type: ExchangeType.Topic, durable: true, autoDelete: false);
            _channel.QueueDeclare(queue: "user_created_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.QueueBind(queue: "user_created_queue", exchange: "user_exchange", routingKey: "user.created");

            _logger.LogInformation("Notification Consumer Service initialized and connected to RabbitMQ.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    // Deserialize the message if it's in JSON format
                    // var customerCreatedEvent = JsonConvert.DeserializeObject<CustomerCreatedEvent>(message);

                    _logger.LogInformation("Received message: {Message}", message);

                    // Implement your notification logic here
                    // Example: Send an email or push notification to the user
                    SendNotification(message);

                    // Acknowledge the message
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", message);
                    // Optionally, reject and requeue the message
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }

                await Task.Yield();
            };

            _channel.BasicConsume(queue: "user_created_queue",
                                  autoAck: false,
                                  consumer: consumer);

            _logger.LogInformation("Notification Consumer Service is listening for messages.");

            return Task.CompletedTask;
        }

        private void SendNotification(string message)
        {
            // Implement your notification logic here
            // For example, sending an email or push notification
            _logger.LogInformation("Sending notification: {Message}", message);
            // Example: EmailService.SendEmail(...)
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
