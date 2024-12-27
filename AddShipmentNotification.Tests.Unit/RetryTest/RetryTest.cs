using interview.Retry;
using JetBrains.Annotations;
using Moq;

namespace AddShipmentNotification.Tests.Unit.RetryTest;

[TestSubject(typeof(Retry))]
public class RetryTest
{
    private const int FIRST_ATTEMPT = 1;
    private const int SECOND_ATTEMPT = 2;
    private const int THIRD_ATTEMPT = 3;
    private const int FOURTH_ATTEMPT = 4;

    public Mock<Func<TimeSpan, Task>> CreateMockDelayFn()
    {
        return new Mock<Func<TimeSpan, Task>>();
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningTrue_LoopsOnceReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(FIRST_ATTEMPT);
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(1, fakeRetryTaskFn.AttemptCount);
        Assert.True(result);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseTrue_LoopsTwiceReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(SECOND_ATTEMPT);
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(2, fakeRetryTaskFn.AttemptCount);
        Assert.True(result);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseTrue_LoopsThreeTimesReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(THIRD_ATTEMPT);
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(3, fakeRetryTaskFn.AttemptCount);
        Assert.True(result);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseFalse_LoopsThreeTimesReturnsFalse()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(FOURTH_ATTEMPT);
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(3, fakeRetryTaskFn.AttemptCount);
        Assert.False(result);
    }

    [Fact]
    public async Task Attempt_WithTwoMaxRetries_LoopsTwiceReturnsFalse()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(THIRD_ATTEMPT);
        IRetry retry = new Retry() { MaxRetries = 2, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(2, fakeRetryTaskFn.AttemptCount);
        Assert.False(result);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningTrue_DoesNotCallsDelayFunction()
    {
        var delayFnMock = CreateMockDelayFn();
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(FIRST_ATTEMPT);
        IRetry retry = new Retry()
        {
            MaxRetries = 3,
            DelaySeconds = 0,
            DelayFn = delayFnMock.Object,
        };

        await retry.Attempt(fakeRetryFn.RetryTask);

        delayFnMock.Verify(x => x(TimeSpan.FromSeconds(0)), Times.Exactly(0));
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseTrue_CallsDelayFunctionOnce()
    {
        var delayFnMock = CreateMockDelayFn();
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(SECOND_ATTEMPT);
        IRetry retry = new Retry()
        {
            MaxRetries = 3,
            DelaySeconds = 0,
            DelayFn = delayFnMock.Object,
        };

        await retry.Attempt(fakeRetryFn.RetryTask);

        delayFnMock.Verify(x => x(TimeSpan.FromSeconds(0)), Times.Exactly(1));
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseTrue_CallsDelayFunctionOnce()
    {
        var delayFnMock = CreateMockDelayFn();
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(THIRD_ATTEMPT);
        IRetry retry = new Retry()
        {
            MaxRetries = 3,
            DelaySeconds = 0,
            DelayFn = delayFnMock.Object,
        };

        await retry.Attempt(fakeRetryFn.RetryTask);

        delayFnMock.Verify(x => x(TimeSpan.FromSeconds(0)), Times.Exactly(2));
    }

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
}
