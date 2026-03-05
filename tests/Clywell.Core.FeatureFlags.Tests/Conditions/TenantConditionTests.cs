namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class TenantConditionTests
{
    [Fact]
    public void Constructor_SingleTenantId_MatchesCorrectly()
    {
        var sut = new TenantCondition("tenant-1");
        var context = new EvaluationContextBuilder().WithTenant("tenant-1").Build();
        Assert.True(sut.Matches(context));
    }

    [Fact]
    public void Matches_TenantInAllowedSet_ReturnsTrue()
    {
        var sut = new TenantCondition(["tenant-a", "tenant-b"]);
        var context = new EvaluationContextBuilder().WithTenant("tenant-a").Build();

        var result = sut.Matches(context);

        Assert.True(result);
    }

    [Fact]
    public void Matches_TenantCasingDifferent_ReturnsTrue()
    {
        var sut = new TenantCondition(["TENANT-A"]);
        var context = new EvaluationContextBuilder().WithTenant("tenant-a").Build();

        var result = sut.Matches(context);

        Assert.True(result);
    }

    [Fact]
    public void Matches_TenantNotInAllowedSet_ReturnsFalse()
    {
        var sut = new TenantCondition(["tenant-a"]);
        var context = new EvaluationContextBuilder().WithTenant("tenant-b").Build();

        var result = sut.Matches(context);

        Assert.False(result);
    }

    [Fact]
    public void Matches_ContextTenantIdIsNull_ReturnsFalse()
    {
        var sut = new TenantCondition(["tenant-a"]);

        var result = sut.Matches(EvaluationContext.Empty);

        Assert.False(result);
    }

    [Fact]
    public void Matches_AllowedSetIsEmpty_ReturnsFalse()
    {
        var sut = new TenantCondition([]);
        var context = new EvaluationContextBuilder().WithTenant("tenant-a").Build();

        var result = sut.Matches(context);

        Assert.False(result);
    }

    [Fact]
    public void Constructor_NullTenantIds_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TenantCondition(null!));
    }

}