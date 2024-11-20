namespace NotificationBrokerService.Services;

public interface IMessageBrokerService
{
    void Publish(string exchange, string routingKey, string message);
}