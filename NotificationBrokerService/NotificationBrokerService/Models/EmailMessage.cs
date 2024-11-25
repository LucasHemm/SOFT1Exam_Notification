namespace NotificationBrokerService.Models;

public class EmailMessage
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
}