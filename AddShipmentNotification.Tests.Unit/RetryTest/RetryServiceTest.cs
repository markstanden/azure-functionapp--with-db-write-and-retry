using AddShipmentNotification.Tests.Unit.FakeRetryFn;
using interview.Services.Retry;
using JetBrains.Annotations;
using Moq;

namespace AddShipmentNotification.Tests.Unit.RetryTest;

[TestSubject(typeof(RetryService))]
public class RetryServiceTest
{
    private const int FirstAttempt = 1;
    private const int SecondAttempt = 2;
    private const int ThirdAttempt = 3;
    private const int FourthAttempt = 4;

    // Convenience delegate to reduce boilerplate within tests
    private readonly Func<int, IRetryService> _instantRetry = (attempts) =>
        new RetryService { MaxRetries = attempts, DelaySeconds = 0 };

    private Mock<Func<TimeSpan, Task>> CreateMockDelayFn()
    {
        return new Mock<Func<TimeSpan, Task>>();
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningTrue_LoopsOnceReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(FirstAttempt);
        IRetryService retryService = _instantRetry(3);

        var result = await retryService.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(1, fakeRetryTaskFn.ExecutionCount);
        Assert.True(result.success);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseTrue_LoopsTwiceReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(SecondAttempt);
        IRetryService retryService = _instantRetry(3);

        var result = await retryService.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(2, fakeRetryTaskFn.ExecutionCount);
        Assert.True(result.success);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseTrue_LoopsThreeTimesReturnsTrue()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(ThirdAttempt);
        IRetryService retryService = _instantRetry(3);

        var result = await retryService.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(3, fakeRetryTaskFn.ExecutionCount);
        Assert.True(result.success);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseFalse_LoopsThreeTimesReturnsFalse()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(FourthAttempt);
        IRetryService retryService = _instantRetry(3);

        var result = await retryService.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(3, fakeRetryTaskFn.ExecutionCount);
        Assert.False(result.success);
    }

    [Fact]
    public async Task Attempt_WithTwoMaxRetries_LoopsTwiceReturnsFalse()
    {
        var fakeRetryTaskFn = FakeRetryTaskFn.CreateSuccessOn(ThirdAttempt);
        IRetryService retryService = _instantRetry(2);

        var result = await retryService.Attempt(fakeRetryTaskFn.RetryTask);

        Assert.Equal(2, fakeRetryTaskFn.ExecutionCount);
        Assert.False(result.success);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningTrue_DoesNotCallsDelayFunction()
    {
        var delayFnMock = CreateMockDelayFn();
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(FirstAttempt);
        IRetryService retryService = new RetryService()
        {
            MaxRetries = 3,
            DelaySeconds = 0,
            DelayFn = delayFnMock.Object,
        };

        await retryService.Attempt(fakeRetryFn.RetryTask);

        delayFnMock.Verify(x => x(TimeSpan.FromSeconds(0)), Times.Exactly(0));
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseTrue_CallsDelayFunctionOnce()
    {
        var delayFnMock = CreateMockDelayFn();
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(SecondAttempt);
        IRetryService retryService = new RetryService()
        {
            MaxRetries = 3,
            DelaySeconds = 0,
            DelayFn = delayFnMock.Object,
        };

        await retryService.Attempt(fakeRetryFn.RetryTask);

        delayFnMock.Verify(x => x(TimeSpan.FromSeconds(0)), Times.Exactly(1));
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseTrue_CallsDelayFunctionOnce()
    {
        var delayFnMock = CreateMockDelayFn();
        var fakeRetryFn = FakeRetryTaskFn.CreateSuccessOn(ThirdAttempt);
        IRetryService retryService = new RetryService()
        {
            MaxRetries = 3,
            DelaySeconds = 0,
            DelayFn = delayFnMock.Object,
        };

        await retryService.Attempt(fakeRetryFn.RetryTask);

        delayFnMock.Verify(x => x(TimeSpan.FromSeconds(0)), Times.Exactly(2));
    }
}
