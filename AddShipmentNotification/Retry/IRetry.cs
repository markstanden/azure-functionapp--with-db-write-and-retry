using interview.Retry;

public interface IRetry
{
    int MaxRetries { get; init; }
    Task<IRetryable> Attempt(Func<Task<IRetryable>> fnAsync, int attempt = 1);
}
