using interview.Infrastructure.Http;
using Microsoft.Extensions.Logging;

namespace interview.Services.Webhook;

public class WebhookService : IWebhookService
{
    private readonly IHttpClientWrapper _httpClient;
    private readonly ILogger<WebhookService> _logger;
    private readonly string _url;

    public WebhookService(IHttpClientWrapper httpClient, ILogger<WebhookService> logger, string url)
    {
        _httpClient = httpClient;
        _logger = logger;
        _url = url;
    }

    /// <summary>
    /// Method sends an HTTP Request to the provided url to
    /// register the successful addition of the record to the DB
    /// </summary>
    /// <param name="query"></param>
    /// <param name="message"></param>
    /// <param name="url"></param>
    public async Task SendMessage(string query, string message)
    {
        await _httpClient.GetAsync($"{_url}?{query}={message}");
        _logger.LogInformation($"Webhook service sent success message to {_url}");
    }
}
