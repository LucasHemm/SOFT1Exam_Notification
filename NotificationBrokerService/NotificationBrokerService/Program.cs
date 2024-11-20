using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationBrokerService.Models;
using NotificationBrokerService.Services;
using NotificationService.Services;
using RabbitMQBrokerService.Services;

namespace NotificationBrokerService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Bind RabbitMQ settings
        builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

// Register RabbitMQ service
        builder.Services.AddSingleton<IMessageBrokerService, MessageBrokerService>();

// Register Hosted Services (Consumers)
        builder.Services.AddHostedService<NotificationConsumerService>();

        var app = builder.Build();

// No endpoints needed unless for health checks or monitoring

        app.Run();
    }
}