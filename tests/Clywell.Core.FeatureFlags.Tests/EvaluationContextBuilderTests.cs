namespace Clywell.Core.FeatureFlags.Tests;

public sealed class EvaluationContextBuilderTests
{
    [Fact]
    public void Build_FreshBuilder_ReturnsEmptyContext()
    {
        var context = new EvaluationContextBuilder().Build();

        Assert.Null(context.TenantId);
        Assert.Null(context.UserId);
        Assert.Empty(context.Attributes);
    }

    [Fact]
    public void WithTenant_ValidTenantId_SetsTenantId()
    {
        var context = new EvaluationContextBuilder()
            .WithTenant("t1")
            .Build();

        Assert.Equal("t1", context.TenantId);
    }

    [Fact]
    public void WithUser_ValidUserId_SetsUserId()
    {
        var context = new EvaluationContextBuilder()
            .WithUser("u1")
            .Build();

        Assert.Equal("u1", context.UserId);
    }

    [Fact]
    public void WithAttribute_ValidKeyAndValue_SetsAttribute()
    {
        var context = new EvaluationContextBuilder()
            .WithAttribute("plan", "enterprise")
            .Build();

        Assert.Equal("enterprise", context.Attributes["plan"]);
    }

    [Fact]
    public void WithAttribute_SameKeyTwice_LastValueWins()
    {
        var context = new EvaluationContextBuilder()
            .WithAttribute("plan", "starter")
            .WithAttribute("plan", "enterprise")
            .Build();

        Assert.Equal("enterprise", context.Attributes["plan"]);
    }

    [Fact]
    public void WithTenant_NullTenant_ThrowsArgumentNullException()
    {
        var sut = new EvaluationContextBuilder();

        Assert.Throws<ArgumentNullException>(() => sut.WithTenant(null!));
    }

    [Fact]
    public void WithTenant_EmptyTenant_ThrowsArgumentException()
    {
        var sut = new EvaluationContextBuilder();

        Assert.Throws<ArgumentException>(() => sut.WithTenant(string.Empty));
    }

    [Fact]
    public void WithTenant_WhitespaceTenant_ThrowsArgumentException()
    {
        var sut = new EvaluationContextBuilder();

        Assert.Throws<ArgumentException>(() => sut.WithTenant("  "));
    }

    [Fact]
    public void WithUser_NullUser_ThrowsArgumentNullException()
    {
        var sut = new EvaluationContextBuilder();

        Assert.Throws<ArgumentNullException>(() => sut.WithUser(null!));
    }

    [Fact]
    public void WithUser_EmptyUser_ThrowsArgumentException()
    {
        var sut = new EvaluationContextBuilder();

        Assert.Throws<ArgumentException>(() => sut.WithUser(string.Empty));
    }

    [Fact]
    public void WithUser_Whitespace_ThrowsArgumentException()
    {
        var builder = new EvaluationContextBuilder();
        Assert.Throws<ArgumentException>(() => builder.WithUser("   "));
    }

    [Fact]
    public void WithAttribute_NullKey_ThrowsArgumentNullException()
    {
        var sut = new EvaluationContextBuilder();

        Assert.Throws<ArgumentNullException>(() => sut.WithAttribute(null!, "v"));
    }

    [Fact]
    public void WithAttribute_EmptyKey_ThrowsArgumentException()
    {
        var sut = new EvaluationContextBuilder();

        Assert.Throws<ArgumentException>(() => sut.WithAttribute(string.Empty, "v"));
    }

    [Fact]
    public void WithAttribute_WhitespaceKey_ThrowsArgumentException()
    {
        var builder = new EvaluationContextBuilder();
        Assert.Throws<ArgumentException>(() => builder.WithAttribute("   ", "value"));
    }
}