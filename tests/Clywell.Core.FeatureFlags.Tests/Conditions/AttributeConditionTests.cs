namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class AttributeConditionTests
{
    [Fact]
    public void Matches_KeyExistsWithCorrectValue_ReturnsTrue()
    {
        var context = new EvaluationContextBuilder()
            .WithAttribute("plan", "enterprise")
            .Build();

        var sut = new AttributeCondition("plan", "enterprise");

        var result = sut.Matches(context);

        Assert.True(result);
    }

    [Fact]
    public void Matches_KeyExistsWithWrongValue_ReturnsFalse()
    {
        var context = new EvaluationContextBuilder()
            .WithAttribute("plan", "starter")
            .Build();

        var sut = new AttributeCondition("plan", "enterprise");

        var result = sut.Matches(context);

        Assert.False(result);
    }

    [Fact]
    public void Matches_KeyAbsentFromAttributes_ReturnsFalse()
    {
        var sut = new AttributeCondition("plan", "enterprise");

        var result = sut.Matches(EvaluationContext.Empty);

        Assert.False(result);
    }

    [Fact]
    public void Matches_DefaultComparisonIsOrdinalIgnoreCase_ReturnsTrueForDifferentCase()
    {
        var context = new EvaluationContextBuilder()
            .WithAttribute("plan", "Enterprise")
            .Build();

        var sut = new AttributeCondition("plan", "enterprise");

        var result = sut.Matches(context);

        Assert.True(result);
    }

    [Fact]
    public void Matches_ExplicitOrdinalComparison_ReturnsFalseForDifferentCase()
    {
        var context = new EvaluationContextBuilder()
            .WithAttribute("plan", "Enterprise")
            .Build();

        var sut = new AttributeCondition("plan", "enterprise", StringComparison.Ordinal);

        var result = sut.Matches(context);

        Assert.False(result);
    }

    [Fact]
    public void Constructor_NullKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AttributeCondition(null!, "value"));
    }

    [Fact]
    public void Constructor_EmptyKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AttributeCondition(string.Empty, "value"));
    }

}