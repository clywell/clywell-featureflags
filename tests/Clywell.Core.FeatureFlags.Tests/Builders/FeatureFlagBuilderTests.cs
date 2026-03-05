namespace Clywell.Core.FeatureFlags.Tests.Builders;

public sealed class FeatureFlagBuilderTests
{
    [Fact]
    public void For_ValidKey_SetsKey()
    {
        var flag = FeatureFlagBuilder.For("my-flag").Build();
        Assert.Equal("my-flag", flag.Key);
    }

    [Fact]
    public void For_NullKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FeatureFlagBuilder.For(null!));
    }

    [Fact]
    public void For_EmptyKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => FeatureFlagBuilder.For(""));
    }

    [Fact]
    public void WithDescription_SetsDescription()
    {
        var flag = FeatureFlagBuilder.For("key").WithDescription("desc").Build();
        Assert.Equal("desc", flag.Description);
    }

    [Fact]
    public void EnabledByDefault_SetsDefaultValueTrue()
    {
        var flag = FeatureFlagBuilder.For("key").EnabledByDefault().Build();
        Assert.True(flag.DefaultValue);
    }

    [Fact]
    public void DisabledByDefault_SetsDefaultValueFalse()
    {
        var flag = FeatureFlagBuilder.For("key").DisabledByDefault().Build();
        Assert.False(flag.DefaultValue);
    }

    [Fact]
    public void Build_NoRules_ReturnsEmptyRulesList()
    {
        var flag = FeatureFlagBuilder.For("key").Build();
        Assert.Empty(flag.Rules);
    }

    [Fact]
    public void EnableWhen_AddsRuleWithValueTrue()
    {
        var flag = FeatureFlagBuilder.For("key")
            .EnableWhen(AlwaysCondition.Instance)
            .Build();

        var rule = Assert.Single(flag.Rules);
        Assert.True(rule.Value);
    }

    [Fact]
    public void DisableWhen_AddsRuleWithValueFalse()
    {
        var flag = FeatureFlagBuilder.For("key")
            .DisableWhen(AlwaysCondition.Instance)
            .Build();

        var rule = Assert.Single(flag.Rules);
        Assert.False(rule.Value);
    }

    [Fact]
    public void EnableWhen_TwoRules_FirstAddedHasHigherPriority()
    {
        var flag = FeatureFlagBuilder.For("key")
            .EnableWhen(AlwaysCondition.Instance)   // first -> priority 20
            .EnableWhen(AlwaysCondition.Instance)   // second -> priority 10
            .Build();

        Assert.Equal(2, flag.Rules.Count);
        Assert.True(flag.Rules[0].Priority > flag.Rules[1].Priority);
    }

    [Fact]
    public void EnableWhen_ExplicitPriority_UsesExplicitValue()
    {
        var flag = FeatureFlagBuilder.For("key")
            .EnableWhen(AlwaysCondition.Instance, priority: 999)
            .Build();

        Assert.Equal(999, flag.Rules[0].Priority);
    }

    [Fact]
    public void EnableWhen_NullCondition_ThrowsArgumentNullException()
    {
        var builder = FeatureFlagBuilder.For("key");
        Assert.Throws<ArgumentNullException>(() => builder.EnableWhen(null!));
    }

    [Fact]
    public void Build_CalledTwice_ReturnsSeparateInstances()
    {
        var builder = FeatureFlagBuilder.For("key")
            .EnableWhen(AlwaysCondition.Instance);
        var flag1 = builder.Build();
        var flag2 = builder.Build();
        Assert.NotSame(flag1, flag2);
    }
}
