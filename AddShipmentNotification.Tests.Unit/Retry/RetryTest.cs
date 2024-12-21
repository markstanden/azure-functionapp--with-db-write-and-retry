using JetBrains.Annotations;
using Moq;

namespace AddShipmentNotification.Tests.Unit.Retry;

[TestSubject(typeof(interview.Retry.Retry))]
public class RetryTest
{
    [Fact]
    public void Attempt_WithFunctionReturningTrue_LoopsOnceRetunsTrue()
    {
        IRetry retry = new interview.Retry.Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.Setup(f => f()).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Once);
    }

    [Fact]
    public void Attempt_WithFunctionReturningFalseTrue_LoopsTwiceReturnsTrue()
    {
        IRetry retry = new interview.Retry.Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Exactly(2));
    }

    [Fact]
    public void Attempt_WithFunctionReturningFalseFalseTrue_LoopsThreeTimesReturnsTrue()
    {
        IRetry retry = new interview.Retry.Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(false).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Exactly(3));
    }

    [Fact]
    public void Attempt_WithFunctionReturningFalseFalseFalse_LoopsThreeTimesReturnsFalse()
    {
        IRetry retry = new interview.Retry.Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(false).Returns(false);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.False(result);
        returnsTrue.Verify(x => x(), Times.Exactly(3));
    }

    [Fact]
    public void Attempt_WithTwoMaxRetries_LoopsTwiceReturnsFalse()
    {
        IRetry retry = new interview.Retry.Retry() { MaxRetries = 2 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(false);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.False(result);
        returnsTrue.Verify(x => x(), Times.Exactly(2));
    }
}
