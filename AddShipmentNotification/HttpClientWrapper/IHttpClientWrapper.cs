namespace interview.HttpClientWrapper;

public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string url);
}
