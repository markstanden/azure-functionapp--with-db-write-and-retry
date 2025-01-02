namespace interview.Retry;

public class Retry : IRetry
{
    public int DelaySeconds { get; init; } = 10;

    public Func<TimeSpan, Task> DelayFn { get; init; } =
        async (TimeSpan timespan) => await Task.Delay(timespan);

    public int MaxRetries { get; init; } = 3;

    public async Task<IRetryable> Attempt(Func<Task<IRetryable>> fnAsync, int attempt = 1)
    {
        var result = await fnAsync();
        if (result.success)
        {
            return result;
        }

        if (attempt >= MaxRetries)
        {
            return result;
        }

        await DelayFn(TimeSpan.FromSeconds(DelaySeconds));

        // Recursively call with an incremented attempt number
        return await Attempt(fnAsync, attempt + 1);
    }
}
