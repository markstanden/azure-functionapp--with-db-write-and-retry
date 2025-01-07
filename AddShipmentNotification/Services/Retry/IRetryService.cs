namespace AddShipmentNotification.Services.Retry;

public interface IRetryService
{
    int MaxRetries { get; init; }
    Task<IRetryable> Attempt(Func<Task<IRetryable>> fnAsync, int attempt = 1);
}
