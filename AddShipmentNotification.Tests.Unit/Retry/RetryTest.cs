using JetBrains.Annotations;
using Moq;

namespace AddShipmentNotification.Tests.Unit.Retry;

[TestSubject(typeof(interview.Retry.Retry))]
public class RetryTest
{
    [Fact]
    public void Attempt_WithFunctionReturningTrue_LoopsOnce()
    {
        IRetry retry = new interview.Retry.Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.Setup(f => f()).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Once);
    }

    [Fact]
    public void Attempt_WithFunctionReturningFalseTrue_LoopsTwice()
    {
        IRetry retry = new interview.Retry.Retry() { MaxRetries = 3 };
        var returnsTrue = new Mock<Func<bool>>();
        returnsTrue.SetupSequence(f => f()).Returns(false).Returns(true);

        var result = retry.Attempt(returnsTrue.Object);

        Assert.True(result);
        returnsTrue.Verify(x => x(), Times.Exactly(2));
    }
}
