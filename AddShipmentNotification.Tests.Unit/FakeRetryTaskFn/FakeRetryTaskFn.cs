namespace AddShipmentNotification.Tests.Unit.FakeRetryFn;

public class FakeRetryTaskFn
{
    private FakeRetryTaskFn(int expectedAttemptCount)
    {
        RetryTask = () =>
        {
            AttemptCount++;
            return Task.FromResult(AttemptCount == expectedAttemptCount);
        };
    }

    public int AttemptCount { get; private set; }
    public Func<Task<bool>> RetryTask { get; }

    public static FakeRetryTaskFn CreateSuccessOn(int attemptNumber) =>
        new FakeRetryTaskFn(attemptNumber);
}
