namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class NotConditionTests
{
    [Fact]
    public void Matches_InnerReturnsTrue_ReturnsFalse()
    {
        var sut = new NotCondition(AlwaysCondition.Instance);
        Assert.False(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Matches_InnerReturnsFalse_ReturnsTrue()
    {
        var never = new Mock<IEvaluationCondition>();
        never.Setup(c => c.Matches(It.IsAny<EvaluationContext>())).Returns(false);

        var sut = new NotCondition(never.Object);
        Assert.True(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Constructor_NullInner_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new NotCondition(null!));
    }
}
