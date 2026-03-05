namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class AnyOfConditionTests
{
    [Fact]
    public void Matches_EmptyConditionSet_ReturnsFalse()
    {
        var sut = new AnyOfCondition([]);

        var result = sut.Matches(EvaluationContext.Empty);

        Assert.False(result);
    }

    [Fact]
    public void Matches_SingleMatchingCondition_ReturnsTrue()
    {
        var sut = new AnyOfCondition([AlwaysCondition.Instance]);
        Assert.True(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Matches_SingleNonMatchingCondition_ReturnsFalse()
    {
        var never = new Mock<IEvaluationCondition>();
        never.Setup(c => c.Matches(It.IsAny<EvaluationContext>())).Returns(false);

        var sut = new AnyOfCondition([never.Object]);
        Assert.False(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Matches_OneMatchesOneDoesNot_ReturnsTrue()
    {
        var never = new Mock<IEvaluationCondition>();
        never.Setup(c => c.Matches(It.IsAny<EvaluationContext>())).Returns(false);

        var sut = new AnyOfCondition([never.Object, AlwaysCondition.Instance]);
        Assert.True(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Matches_NoneMatch_ReturnsFalse()
    {
        var never = new Mock<IEvaluationCondition>();
        never.Setup(c => c.Matches(It.IsAny<EvaluationContext>())).Returns(false);

        var sut = new AnyOfCondition([never.Object, never.Object]);
        Assert.False(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Constructor_NullConditions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AnyOfCondition(null!));
    }
}
