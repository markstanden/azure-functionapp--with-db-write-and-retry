namespace AddShipmentNotification.Services.Webhook;

public interface IWebhookService
{
    public Task SendMessage(string query, string message);
}
