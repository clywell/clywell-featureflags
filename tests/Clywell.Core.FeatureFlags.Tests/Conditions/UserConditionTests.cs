namespace Clywell.Core.FeatureFlags.Tests.Conditions;

public sealed class UserConditionTests
{
    [Fact]
    public void Constructor_SingleUserId_MatchesCorrectly()
    {
        var sut = new UserCondition("user-1");
        var context = new EvaluationContextBuilder().WithUser("user-1").Build();
        Assert.True(sut.Matches(context));
    }

    [Fact]
    public void Matches_UserInAllowedSet_ReturnsTrue()
    {
        var sut = new UserCondition(["user-a", "user-b"]);
        var context = new EvaluationContextBuilder().WithUser("user-a").Build();

        var result = sut.Matches(context);

        Assert.True(result);
    }

    [Fact]
    public void Matches_UserCasingDifferent_ReturnsTrue()
    {
        var sut = new UserCondition(["USER-A"]);
        var context = new EvaluationContextBuilder().WithUser("user-a").Build();

        var result = sut.Matches(context);

        Assert.True(result);
    }

    [Fact]
    public void Matches_UserNotInAllowedSet_ReturnsFalse()
    {
        var sut = new UserCondition(["user-a"]);
        var context = new EvaluationContextBuilder().WithUser("user-b").Build();

        var result = sut.Matches(context);

        Assert.False(result);
    }

    [Fact]
    public void Matches_ContextUserIdIsNull_ReturnsFalse()
    {
        var sut = new UserCondition(["user-a"]);

        var result = sut.Matches(EvaluationContext.Empty);

        Assert.False(result);
    }

    [Fact]
    public void Matches_AllowedSetIsEmpty_ReturnsFalse()
    {
        var sut = new UserCondition([]);
        var context = new EvaluationContextBuilder().WithUser("user-a").Build();

        var result = sut.Matches(context);

        Assert.False(result);
    }

    [Fact]
    public void Constructor_NullUserIds_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UserCondition(null!));
    }

}