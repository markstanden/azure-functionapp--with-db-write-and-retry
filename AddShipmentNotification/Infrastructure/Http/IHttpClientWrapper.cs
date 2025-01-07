namespace AddShipmentNotification.Infrastructure.Http;

public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string url);
}
