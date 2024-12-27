using AddShipmentNotification.Tests.Unit.FakeRetryFn;
using interview.Retry;
using JetBrains.Annotations;
using Moq;

namespace AddShipmentNotification.Tests.Unit.RetryTest;

[TestSubject(typeof(Retry))]
public class RetryTest
{
    private const int FirstAttempt = 1;
    private const int SecondAttempt = 2;
    private const int ThirdAttempt = 3;
    private const int FourthAttempt = 4;

    public Mock<Func<TimeSpan, Task>> CreateMockDelayFn()
    {
        return new Mock<Func<TimeSpan, Task>>();
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningTrue_LoopsOnceReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(FirstAttempt);
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(1, fakeRetryTaskFn.AttemptCount);
        Assert.True(result);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseTrue_LoopsTwiceReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(SecondAttempt);
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(2, fakeRetryTaskFn.AttemptCount);
        Assert.True(result);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseTrue_LoopsThreeTimesReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(ThirdAttempt);
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(3, fakeRetryTaskFn.AttemptCount);
        Assert.True(result);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseFalse_LoopsThreeTimesReturnsFalse()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(FourthAttempt);
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(3, fakeRetryTaskFn.AttemptCount);
        Assert.False(result);
    }

    [Fact]
    public async Task Attempt_WithTwoMaxRetries_LoopsTwiceReturnsFalse()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(ThirdAttempt);
        IRetry retry = new Retry() { MaxRetries = 2, DelaySeconds = 0 };

        var result = await retry.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(2, fakeRetryTaskFn.AttemptCount);
        Assert.False(result);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningTrue_DoesNotCallsDelayFunction()
    {
        var delayFnMock = CreateMockDelayFn();
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(FirstAttempt);
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
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(SecondAttempt);
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
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(ThirdAttempt);
        IRetry retry = new Retry()
        {
            MaxRetries = 3,
            DelaySeconds = 0,
            DelayFn = delayFnMock.Object,
        };

        await retry.Attempt(fakeRetryFn.RetryTask);

        delayFnMock.Verify(x => x(TimeSpan.FromSeconds(0)), Times.Exactly(2));
    }
}
