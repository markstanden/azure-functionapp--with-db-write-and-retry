namespace interview.Retry;

public class Retry : IRetry
{
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

        // Recursively call with an incremented attempt number
        return Attempt(fn, attempt + 1);
    }
}
