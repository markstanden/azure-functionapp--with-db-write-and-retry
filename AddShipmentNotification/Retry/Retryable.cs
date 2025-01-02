namespace interview.Retry;

public record Retryable : IRetryable
{
    public required bool success { get; init; }
    public required string message { get; init; }
}
