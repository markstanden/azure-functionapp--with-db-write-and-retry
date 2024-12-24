using interview.Retry;
using JetBrains.Annotations;
using Moq;

namespace AddShipmentNotification.Tests.Unit.RetryTest;

[TestSubject(typeof(Retry))]
public class RetryTest
{
    [Fact]
    public void Attempt_WithFunctionReturningTrue_LoopsOnceReturnsTrue()
    {
        IRetry retry = new Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.Setup(f => f()).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Once);
    }

    [Fact]
    public void Attempt_WithFunctionReturningFalseTrue_LoopsTwiceReturnsTrue()
    {
        IRetry retry = new Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Exactly(2));
    }

    [Fact]
    public void Attempt_WithFunctionReturningFalseFalseTrue_LoopsThreeTimesReturnsTrue()
    {
        IRetry retry = new Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(false).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Exactly(3));
    }

    [Fact]
    public void Attempt_WithFunctionReturningFalseFalseFalse_LoopsThreeTimesReturnsFalse()
    {
        IRetry retry = new Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(false).Returns(false);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.False(result);
        returnsTrue.Verify(x => x(), Times.Exactly(3));
    }

    [Fact]
    public void Attempt_WithTwoMaxRetries_LoopsTwiceReturnsFalse()
    {
        IRetry retry = new Retry() { MaxRetries = 2 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(false);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.False(result);
        returnsTrue.Verify(x => x(), Times.Exactly(2));
    }

    [Fact]
    public void Attempt_WithFunctionReturningFalseTrue_CallsDelayFunctionOnce()
    {
        var delayFn = new Mock<Action<TimeSpan>>();

        IRetry retry = new Retry()
        {
            MaxRetries = 3,
            DelaySeconds = 10,
            DelayFn = delayFn.Object,
        };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        delayFn.Verify(x => x(TimeSpan.FromSeconds(10)), Times.Exactly(1));
        returnsTrue.Verify(x => x(), Times.Exactly(2));
    }
}
