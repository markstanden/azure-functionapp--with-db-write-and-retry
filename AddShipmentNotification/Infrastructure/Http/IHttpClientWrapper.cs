namespace interview.Infrastructure.Http;

public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string url);
}
