using interview.Retry;
using JetBrains.Annotations;
using Moq;

namespace AddShipmentNotification.Tests.Unit.RetryTest;

[TestSubject(typeof(Retry))]
public class RetryTest
{
    public Mock<Func<TimeSpan, Task>> CreateMockDelayFn()
    {
        return new Mock<Func<TimeSpan, Task>>();
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningTrue_LoopsOnceReturnsTrue()
    {
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.Setup(f => f()).Returns(true);

        var result = await retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseTrue_LoopsTwiceReturnsTrue()
    {
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };
        var returnsFalseTrue = new Mock<Func<bool>>();
        returnsFalseTrue.SetupSequence(f => f()).Returns(false).Returns(true);

        var result = await retry.Attempt(returnsFalseTrue.Object);

        Assert.True(result);
        returnsFalseTrue.Verify(x => x(), Times.Exactly(2));
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseTrue_LoopsThreeTimesReturnsTrue()
    {
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };
        var returnsFalseFalseTrue = new Mock<Func<bool>>();
        returnsFalseFalseTrue.SetupSequence(f => f()).Returns(false).Returns(false).Returns(true);

        var result = await retry.Attempt(returnsFalseFalseTrue.Object);

        Assert.True(result);
        returnsFalseFalseTrue.Verify(x => x(), Times.Exactly(3));
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseFalseFalse_LoopsThreeTimesReturnsFalse()
    {
        IRetry retry = new Retry() { MaxRetries = 3, DelaySeconds = 0 };

        var returnsFalseFalseFalse = new Mock<Func<bool>>();
        returnsFalseFalseFalse.SetupSequence(f => f()).Returns(false).Returns(false).Returns(false);

        var result = await retry.Attempt(returnsFalseFalseFalse.Object);

        Assert.False(result);
        returnsFalseFalseFalse.Verify(x => x(), Times.Exactly(3));
    }

    [Fact]
    public async Task Attempt_WithTwoMaxRetries_LoopsTwiceReturnsFalse()
    {
        IRetry retry = new Retry() { MaxRetries = 2, DelaySeconds = 0 };

        var returnsFalseFalse = new Mock<Func<bool>>();
        returnsFalseFalse.SetupSequence(f => f()).Returns(false).Returns(false);

        var result = await retry.Attempt(returnsFalseFalse.Object);

        Assert.False(result);
        returnsFalseFalse.Verify(x => x(), Times.Exactly(2));
    }

    [Fact]
    public async Task Attempt_WithFunctionReturningFalseTrue_CallsDelayFunctionOnce()
    {
        var delayFnMock = CreateMockDelayFn();
        IRetry retry = new Retry()
        {
            MaxRetries = 3,
            DelaySeconds = 0,
            DelayFn = delayFnMock.Object,
        };
        var returnsFalseTrue = new Mock<Func<bool>>();
        returnsFalseTrue.SetupSequence(f => f()).Returns(false).Returns(true);

        var result = await retry.Attempt(returnsFalseTrue.Object);

        Assert.True(result);
        delayFnMock.Verify(x => x(TimeSpan.FromSeconds(0)), Times.Exactly(1));
        returnsFalseTrue.Verify(x => x(), Times.Exactly(2));
    }
}
