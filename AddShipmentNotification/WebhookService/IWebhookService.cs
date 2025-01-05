namespace interview.WebhookService;

public interface IWebhookService
{
    public Task SendMessage(string query, string message);
}
