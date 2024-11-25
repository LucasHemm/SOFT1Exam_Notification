using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using NotificationBrokerService.Models;

namespace NotificationService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // RabbitMQ connection details
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare the queue
            var queueName = "email_queue";
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Create a consumer
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                // Deserialize the message
                var message = JsonSerializer.Deserialize<EmailMessage>(messageJson);

                Console.WriteLine($"Received message to {message.ToEmail}");

                // Send email
                await SendEmailAsync(message.ToEmail, message.Subject, message.Content);

                // Acknowledge message
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            // Start consuming
            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            Console.WriteLine("Waiting for messages...");
            Console.ReadLine();
        }

        static async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            var apiKey = "YOUR_SENDGRID_API_KEY"; // Replace with your SendGrid API key
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("noreply@yourdomain.com", "Your App");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);

            var response = await client.SendEmailAsync(msg);
            Console.WriteLine($"Email sent to {toEmail}. Status: {response.StatusCode}");
        }
    }
}
