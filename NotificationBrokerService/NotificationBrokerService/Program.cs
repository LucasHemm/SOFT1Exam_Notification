using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using NotificationBrokerService.Models;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace NotificationService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting NotificationBrokerService...");

            var cancellationTokenSource = new CancellationTokenSource();

            // Handle termination signals gracefully
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Cancellation requested. Exiting...");
                eventArgs.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            try
            {
                // Build configuration
                var config = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();

                // RabbitMQ connection details
                var factory = new ConnectionFactory()
                {
                    HostName = config["RabbitMQ:Host"] ?? "localhost",
                    Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
                    UserName = config["RabbitMQ:Username"] ?? "guest",
                    Password = config["RabbitMQ:Password"] ?? "guest"
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                Console.WriteLine("Connected to RabbitMQ successfully.");

                // Declare the queue
                var queueName = "email_queue";
                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                Console.WriteLine($"Queue '{queueName}' declared successfully.");

                // Create a consumer
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

                    Console.WriteLine($"Message received: {messageJson}");

                    try
                    {
                        // Deserialize the message
                        var message = JsonSerializer.Deserialize<EmailMessage>(messageJson);

                        Console.WriteLine($"Processing message for: {message.ToEmail}");

                        // Send email
                        await SendEmailAsync(message.ToEmail, message.Subject, message.Content, config);

                        Console.WriteLine($"Email sent to: {message.ToEmail}");

                        // Acknowledge the message
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                    }
                };

                // Start consuming messages testy
                channel.BasicConsume(queue: queueName,
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine("Waiting for messages... Press Ctrl+C to exit.");

                // Keep the application running
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000); // Check cancellation token periodically
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("NotificationBrokerService is shutting down.");
            }
        }

        static async Task SendEmailAsync(string toEmail, string subject, string content, IConfiguration config)
        {
            var apiKey = config["SendGrid:ApiKey"]; // From environment variables
            var fromEmail = config["SendGrid:FromEmail"] ?? "noreply@yourdomain.com";

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("SendGrid API Key is not configured.");
                return;
            }

            try
            {
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, "MTOGO");
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);

                var response = await client.SendEmailAsync(msg);
                Console.WriteLine($"Email sent to {toEmail}. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {toEmail}: {ex.Message}");
            }
        }
    }
}
