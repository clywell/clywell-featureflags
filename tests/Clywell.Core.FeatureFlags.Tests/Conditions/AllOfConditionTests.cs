namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class AllOfConditionTests
{
    [Fact]
    public void Matches_EmptyConditionSet_ReturnsTrue()
    {
        var sut = new AllOfCondition([]);

        var result = sut.Matches(EvaluationContext.Empty);

        Assert.True(result);
    }

    [Fact]
    public void Matches_SingleMatchingCondition_ReturnsTrue()
    {
        var sut = new AllOfCondition([AlwaysCondition.Instance]);
        Assert.True(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Matches_SingleNonMatchingCondition_ReturnsFalse()
    {
        var never = new Mock<IEvaluationCondition>();
        never.Setup(c => c.Matches(It.IsAny<EvaluationContext>())).Returns(false);

        var sut = new AllOfCondition([never.Object]);
        Assert.False(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Matches_AllConditionsMatch_ReturnsTrue()
    {
        var sut = new AllOfCondition([AlwaysCondition.Instance, AlwaysCondition.Instance]);
        Assert.True(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Matches_OneConditionFails_ReturnsFalse()
    {
        var never = new Mock<IEvaluationCondition>();
        never.Setup(c => c.Matches(It.IsAny<EvaluationContext>())).Returns(false);

        var sut = new AllOfCondition([AlwaysCondition.Instance, never.Object]);
        Assert.False(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Constructor_NullConditions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AllOfCondition(null!));
    }
}
