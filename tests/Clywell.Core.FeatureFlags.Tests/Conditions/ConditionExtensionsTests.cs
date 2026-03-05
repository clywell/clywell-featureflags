namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class ConditionExtensionsTests
{
    private static IEvaluationCondition Never()
    {
        var m = new Mock<IEvaluationCondition>();
        m.Setup(c => c.Matches(It.IsAny<EvaluationContext>())).Returns(false);
        return m.Object;
    }

    // Static factory - AllOf
    [Fact]
    public void AllOf_BothMatch_ReturnsTrue() =>
        Assert.True(Condition.AllOf(AlwaysCondition.Instance, AlwaysCondition.Instance).Matches(EvaluationContext.Empty));

    [Fact]
    public void AllOf_OneFails_ReturnsFalse() =>
        Assert.False(Condition.AllOf(AlwaysCondition.Instance, Never()).Matches(EvaluationContext.Empty));

    [Fact]
    public void AllOf_NullArray_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => Condition.AllOf(null!));

    // Static factory - AnyOf
    [Fact]
    public void AnyOf_OneMatches_ReturnsTrue() =>
        Assert.True(Condition.AnyOf(Never(), AlwaysCondition.Instance).Matches(EvaluationContext.Empty));

    [Fact]
    public void AnyOf_NoneMatch_ReturnsFalse() =>
        Assert.False(Condition.AnyOf(Never(), Never()).Matches(EvaluationContext.Empty));

    [Fact]
    public void AnyOf_NullArray_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => Condition.AnyOf(null!));

    [Fact]
    public void Not_InvertsTrue_ReturnsFalse() =>
        Assert.False(Condition.Not(AlwaysCondition.Instance).Matches(EvaluationContext.Empty));

    [Fact]
    public void Not_InvertsFalse_ReturnsTrue() =>
        Assert.True(Condition.Not(Never()).Matches(EvaluationContext.Empty));

    [Fact]
    public void Not_NullCondition_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => Condition.Not(null!));

    // Extension methods
    [Fact]
    public void And_BothMatch_ReturnsTrue() =>
        Assert.True(AlwaysCondition.Instance.And(AlwaysCondition.Instance).Matches(EvaluationContext.Empty));

    [Fact]
    public void And_OneFails_ReturnsFalse() =>
        Assert.False(AlwaysCondition.Instance.And(Never()).Matches(EvaluationContext.Empty));

    [Fact]
    public void And_NullLeft_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => ((IEvaluationCondition)null!).And(AlwaysCondition.Instance));

    [Fact]
    public void And_NullRight_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => AlwaysCondition.Instance.And(null!));

    [Fact]
    public void Or_OneMatches_ReturnsTrue() =>
        Assert.True(Never().Or(AlwaysCondition.Instance).Matches(EvaluationContext.Empty));

    [Fact]
    public void Or_NoneMatch_ReturnsFalse() =>
        Assert.False(Never().Or(Never()).Matches(EvaluationContext.Empty));

    [Fact]
    public void Or_NullRight_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => AlwaysCondition.Instance.Or(null!));

    [Fact]
    public void Negate_InvertsAlways_ReturnsFalse() =>
        Assert.False(AlwaysCondition.Instance.Negate().Matches(EvaluationContext.Empty));

    [Fact]
    public void Negate_NullCondition_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => ((IEvaluationCondition)null!).Negate());
}
