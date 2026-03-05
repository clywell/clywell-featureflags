namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class PercentageConditionTests
{
    [Fact]
    public void Matches_ZeroPercent_ReturnsFalse()
    {
        var sut = new PercentageCondition("flag-a", 0);
        var context = new EvaluationContextBuilder().WithUser("user-1").Build();

        var result = sut.Matches(context);

        Assert.False(result);
    }

    [Fact]
    public void Matches_HundredPercent_ReturnsTrue()
    {
        var sut = new PercentageCondition("flag-a", 100);
        var context = EvaluationContext.Empty;

        var result = sut.Matches(context);

        Assert.True(result);
    }

    [Fact]
    public void Matches_SameFlagKeyAndUserId_ReturnsDeterministicResult()
    {
        var sut = new PercentageCondition("flag-a", 50);
        var context = new EvaluationContextBuilder().WithUser("user-1").Build();

        var first = sut.Matches(context);
        var second = sut.Matches(context);
        var third = sut.Matches(new EvaluationContextBuilder().WithUser("user-1").Build());

        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }

    [Fact]
    public void Matches_SameFlagKeyAndTenantIdWithoutUserId_ReturnsDeterministicResult()
    {
        var sut = new PercentageCondition("flag-a", 50);
        var context = new EvaluationContextBuilder().WithTenant("tenant-1").Build();

        var first = sut.Matches(context);
        var second = sut.Matches(context);
        var third = sut.Matches(new EvaluationContextBuilder().WithTenant("tenant-1").Build());

        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }

    [Fact]
    public void Matches_DifferentFlagKeysForSameUser_CanProduceDifferentResults()
    {
        var context = new EvaluationContextBuilder().WithUser("user-1").Build();
        var baseline = new PercentageCondition("flag-a", 50).Matches(context);

        bool? foundDifferent = null;

        for (var i = 0; i < 200; i++)
        {
            var candidate = new PercentageCondition($"flag-{i}", 50).Matches(context);
            if (candidate != baseline)
            {
                foundDifferent = candidate;
                break;
            }
        }

        Assert.NotNull(foundDifferent);
        Assert.NotEqual(baseline, foundDifferent!.Value);
    }

    [Fact]
    public void Matches_NullUserIdAndNullTenantId_ReturnsFalse()
    {
        var sut = new PercentageCondition("flag-a", 50);

        var result = sut.Matches(EvaluationContext.Empty);

        Assert.False(result);
    }

    [Fact]
    public void Matches_UserIdNull_UsesTenantIdAsFallbackDiscriminator()
    {
        var sut = new PercentageCondition("flag-a", 50);

        var tenantOnlyContext = new EvaluationContextBuilder()
            .WithTenant("tenant-abc")
            .Build();

        var equivalentUserContext = new EvaluationContextBuilder()
            .WithUser("tenant-abc")
            .Build();

        var tenantResult = sut.Matches(tenantOnlyContext);
        var equivalentUserResult = sut.Matches(equivalentUserContext);

        Assert.Equal(equivalentUserResult, tenantResult);
    }

    [Fact]
    public void Constructor_NegativePercentage_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PercentageCondition("flag-a", -1));
    }

    [Fact]
    public void Constructor_PercentageGreaterThan100_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PercentageCondition("flag-a", 101));
    }

    [Fact]
    public void Constructor_NullFlagKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PercentageCondition(null!, 10));
    }

    [Fact]
    public void Constructor_EmptyFlagKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PercentageCondition(string.Empty, 10));
    }
}