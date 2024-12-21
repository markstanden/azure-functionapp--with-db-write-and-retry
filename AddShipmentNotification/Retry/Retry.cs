namespace interview.Retry;

public class Retry : IRetry
{
    public int MaxRetries { get; init; } = 3;

    public bool Attempt(Func<bool> fn)
    {
        throw new NotImplementedException();
    }
}
