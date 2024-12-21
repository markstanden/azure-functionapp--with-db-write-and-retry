public interface IRetry
{
    int MaxRetries { get; init; }
    bool Attempt(Func<bool> fn, int attempt = 1);
}
