namespace interview.Retry;

public class Retry : IRetry
{
    public int DelaySeconds = 10;

    public Func<TimeSpan, Task> DelayFn { get; init; } =
        async (TimeSpan timespan) => await Task.Delay(timespan);

    public int MaxRetries { get; init; } = 3;

    public async Task<bool> Attempt(Func<Task<bool>> fnAsync, int attempt = 1)
    {
        if (await fnAsync())
        {
            return true;
        }

        if (attempt >= MaxRetries)
        {
            return false;
        }

        await DelayFn(TimeSpan.FromSeconds(DelaySeconds));

        // Recursively call with an incremented attempt number
        return await Attempt(fnAsync, attempt + 1);
    }
}
