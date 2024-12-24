namespace interview.Retry;

public class Retry : IRetry
{
    public int DelaySeconds = 10;
    public Action<TimeSpan> DelayFn { get; init; } = (TimeSpan timeSpan) => Task.Delay(timeSpan);
    public int MaxRetries { get; init; } = 3;

    public bool Attempt(Func<bool> fn, int attempt = 1)
    {
        if (fn())
        {
            return true;
        }

        if (attempt >= MaxRetries)
        {
            return false;
        }

        DelayFn(TimeSpan.FromSeconds(DelaySeconds));

        // Recursively call with an incremented attempt number
        return Attempt(fn, attempt + 1);
    }
}
