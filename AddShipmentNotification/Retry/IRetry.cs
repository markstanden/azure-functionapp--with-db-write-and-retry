public interface IRetry
{
    int MaxRetries { get; init; }
    Task<bool> Attempt(Func<bool> fn, int attempt = 1);
}
