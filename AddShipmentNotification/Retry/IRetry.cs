public interface IRetry
{
    int MaxRetries { get; init; }
    Task<bool> Attempt(Func<Task<bool>> fnAsync, int attempt = 1);
}
