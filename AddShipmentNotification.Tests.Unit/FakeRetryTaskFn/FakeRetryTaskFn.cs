namespace AddShipmentNotification.Tests.Unit.FakeRetryFn;

/// <summary>
/// Factory class producing a Fake retryable function.
/// The class maintains an execution count for the retryable function
/// </summary>
public class FakeRetryTaskFn
{
    /// <summary>
    /// private constructor returns a partially executed retryable function with the
    /// expectedAttemptCount (attempt number when true) baked in.
    ///
    /// Returned function mutates (increments) local ExecutionCount
    /// variable on each call.
    /// </summary>
    /// <param name="expectedAttemptCount">Number of attempts until task resolves true</param>
    private FakeRetryTaskFn(int expectedAttemptCount)
    {
        RetryTask = () =>
        {
            ExecutionCount++;
            return Task.FromResult(ExecutionCount == expectedAttemptCount);
        };
    }

    public int ExecutionCount { get; private set; }
    public Func<Task<bool>> RetryTask { get; }

    /// <summary>
    /// Static factory method to return a prepared class instance.
    /// </summary>
    /// <param name="attemptNumber">Attempt number that task will resolves true</param>
    /// <returns>Custom class instance</returns>
    public static FakeRetryTaskFn CreateSuccessOn(int attemptNumber) =>
        new FakeRetryTaskFn(attemptNumber);
}
