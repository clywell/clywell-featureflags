namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class AlwaysConditionTests
{
    [Fact]
    public void Matches_WithEmptyContext_ReturnsTrue()
    {
        var sut = new AlwaysCondition();
        Assert.True(sut.Matches(EvaluationContext.Empty));
    }

    [Fact]
    public void Matches_WithFullContext_ReturnsTrue()
    {
        var context = new EvaluationContextBuilder()
            .WithTenant("t1")
            .WithUser("u1")
            .WithAttribute("plan", "enterprise")
            .Build();

        var sut = AlwaysCondition.Instance;
        Assert.True(sut.Matches(context));
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        Assert.NotNull(AlwaysCondition.Instance);
    }
}