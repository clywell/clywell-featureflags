namespace Clywell.Core.FeatureFlags.Tests;

public sealed class FeatureFlagServiceExtensionsTests
{
    private readonly Mock<IFeatureFlagService> _service = new();

    [Fact]
    public async Task IsEnabledAsync_WithTenantId_CallsServiceWithTenantContext()
    {
        EvaluationContext? captured = null;
        _service
            .Setup(s => s.IsEnabledAsync("flag", It.IsAny<Action<EvaluationContextBuilder>>(), It.IsAny<CancellationToken>()))
            .Callback<string, Action<EvaluationContextBuilder>, CancellationToken>((_, configure, _) =>
            {
                var builder = new EvaluationContextBuilder();
                configure(builder);
                captured = builder.Build();
            })
            .ReturnsAsync(true);

        await _service.Object.IsEnabledAsync("flag", "tenant-1");

        Assert.NotNull(captured);
        Assert.Equal("tenant-1", captured!.TenantId);
        Assert.Null(captured.UserId);
    }

    [Fact]
    public async Task IsEnabledAsync_WithTenantAndUserId_CallsServiceWithBothInContext()
    {
        EvaluationContext? captured = null;
        _service
            .Setup(s => s.IsEnabledAsync("flag", It.IsAny<Action<EvaluationContextBuilder>>(), It.IsAny<CancellationToken>()))
            .Callback<string, Action<EvaluationContextBuilder>, CancellationToken>((_, configure, _) =>
            {
                var builder = new EvaluationContextBuilder();
                configure(builder);
                captured = builder.Build();
            })
            .ReturnsAsync(true);

        await _service.Object.IsEnabledAsync("flag", "tenant-1", "user-1");

        Assert.NotNull(captured);
        Assert.Equal("tenant-1", captured!.TenantId);
        Assert.Equal("user-1", captured.UserId);
    }

    [Fact]
    public async Task EvaluateManyAsync_WithContext_ReturnsDictionaryForAllKeys()
    {
        _service.Setup(s => s.IsEnabledAsync("flag-a", It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _service.Setup(s => s.IsEnabledAsync("flag-b", It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _service.Object.EvaluateManyAsync(["flag-a", "flag-b"], EvaluationContext.Empty);

        Assert.Equal(2, result.Count);
        Assert.True(result["flag-a"]);
        Assert.False(result["flag-b"]);
    }

    [Fact]
    public async Task EvaluateManyAsync_WithBuilderDelegate_PassesCorrectContext()
    {
        EvaluationContext? captured = null;
        _service
            .Setup(s => s.IsEnabledAsync(It.IsAny<string>(), It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()))
            .Callback<string, EvaluationContext, CancellationToken>((_, ctx, _) => captured = ctx)
            .ReturnsAsync(true);

        await _service.Object.EvaluateManyAsync(["flag-a"], ctx => ctx.WithTenant("t1"));

        Assert.Equal("t1", captured?.TenantId);
    }

    [Fact]
    public async Task IsEnabledAsync_WithTenantId_NullService_ThrowsArgumentNullException()
    {
        IFeatureFlagService? service = null;
        await Assert.ThrowsAsync<ArgumentNullException>(() => service!.IsEnabledAsync("flag", "t1"));
    }

    [Fact]
    public async Task IsEnabledAsync_WithTenantId_NullTenantId_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.Object.IsEnabledAsync("flag", (string)null!));
    }

    [Fact]
    public async Task IsEnabledAsync_WithTenantAndUser_NullUserId_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.Object.IsEnabledAsync("flag", "t1", (string)null!));
    }

    [Fact]
    public async Task EvaluateManyAsync_NullService_ThrowsArgumentNullException()
    {
        IFeatureFlagService? service = null;
        await Assert.ThrowsAsync<ArgumentNullException>(() => service!.EvaluateManyAsync(["key"], EvaluationContext.Empty));
    }

    [Fact]
    public async Task EvaluateManyAsync_NullKeys_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.Object.EvaluateManyAsync(null!, EvaluationContext.Empty));
    }
}
