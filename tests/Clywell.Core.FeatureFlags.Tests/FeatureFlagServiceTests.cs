namespace Clywell.Core.FeatureFlags.Tests;

public sealed class FeatureFlagServiceTests
{
    [Fact]
    public async Task IsEnabledAsync_KeyProviderReturnsNull_ReturnsDefaultNotFoundValueFalseByDefault()
    {
        var provider = new Mock<IFeatureFlagProvider>();
        provider
            .Setup(p => p.GetAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag?)null);

        var evaluator = new Mock<IFeatureFlagEvaluator>(MockBehavior.Strict);
        var options = new FeatureFlagOptions();
        var sut = new FeatureFlagService(provider.Object, evaluator.Object, options);

        var result = await sut.IsEnabledAsync("missing");

        Assert.False(result);
        evaluator.Verify(
            e => e.Evaluate(It.IsAny<FeatureFlag>(), It.IsAny<EvaluationContext>()),
            Times.Never);
    }

    [Fact]
    public async Task IsEnabledAsync_KeyProviderReturnsNullWithConfiguredDefaultTrue_ReturnsTrue()
    {
        var provider = new Mock<IFeatureFlagProvider>();
        provider
            .Setup(p => p.GetAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag?)null);

        var evaluator = new Mock<IFeatureFlagEvaluator>(MockBehavior.Strict);
        var options = new FeatureFlagOptions().WithDefaultValueWhenNotFound(true);
        var sut = new FeatureFlagService(provider.Object, evaluator.Object, options);

        var result = await sut.IsEnabledAsync("missing");

        Assert.True(result);
        evaluator.Verify(
            e => e.Evaluate(It.IsAny<FeatureFlag>(), It.IsAny<EvaluationContext>()),
            Times.Never);
    }

    [Fact]
    public async Task IsEnabledAsync_KeyOnlyProviderReturnsFlag_CallsEvaluatorWithFlagAndEmptyContext()
    {
        var flag = new FeatureFlag { Key = "f1", DefaultValue = false };

        var provider = new Mock<IFeatureFlagProvider>();
        provider
            .Setup(p => p.GetAsync("f1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var evaluator = new Mock<IFeatureFlagEvaluator>();
        evaluator
            .Setup(e => e.Evaluate(flag, It.IsAny<EvaluationContext>()))
            .Returns(true);

        var sut = new FeatureFlagService(provider.Object, evaluator.Object, new FeatureFlagOptions());

        var result = await sut.IsEnabledAsync("f1");

        Assert.True(result);
        evaluator.Verify(
            e => e.Evaluate(
                flag,
                It.Is<EvaluationContext>(c => c.TenantId == null && c.UserId == null && c.Attributes.Count == 0)),
            Times.Once);
    }

    [Fact]
    public async Task IsEnabledAsync_WithContext_CallsEvaluatorWithSuppliedContext()
    {
        var flag = new FeatureFlag { Key = "f1", DefaultValue = false };
        var context = new EvaluationContextBuilder().WithTenant("t1").Build();

        var provider = new Mock<IFeatureFlagProvider>();
        provider
            .Setup(p => p.GetAsync("f1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var evaluator = new Mock<IFeatureFlagEvaluator>();
        evaluator
            .Setup(e => e.Evaluate(flag, context))
            .Returns(true);

        var sut = new FeatureFlagService(provider.Object, evaluator.Object, new FeatureFlagOptions());

        var result = await sut.IsEnabledAsync("f1", context);

        Assert.True(result);
        evaluator.Verify(e => e.Evaluate(flag, context), Times.Once);
    }

    [Fact]
    public async Task IsEnabledAsync_WithConfigureBuilder_CallsEvaluatorWithBuiltTenantContext()
    {
        var flag = new FeatureFlag { Key = "f1", DefaultValue = false };

        var provider = new Mock<IFeatureFlagProvider>();
        provider
            .Setup(p => p.GetAsync("f1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var evaluator = new Mock<IFeatureFlagEvaluator>();
        evaluator
            .Setup(e => e.Evaluate(flag, It.IsAny<EvaluationContext>()))
            .Returns(true);

        var sut = new FeatureFlagService(provider.Object, evaluator.Object, new FeatureFlagOptions());

        var result = await sut.IsEnabledAsync("f1", b => b.WithTenant("t"));

        Assert.True(result);
        evaluator.Verify(
            e => e.Evaluate(
                flag,
                It.Is<EvaluationContext>(c => c.TenantId == "t")),
            Times.Once);
    }

    [Fact]
    public async Task IsEnabledAsync_NullKey_ThrowsArgumentNullException()
    {
        var sut = new FeatureFlagService(
            new Mock<IFeatureFlagProvider>().Object,
            new Mock<IFeatureFlagEvaluator>().Object,
            new FeatureFlagOptions());

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.IsEnabledAsync(null!));
    }

    [Fact]
    public async Task IsEnabledAsync_EmptyKey_ThrowsArgumentException()
    {
        var sut = new FeatureFlagService(
            new Mock<IFeatureFlagProvider>().Object,
            new Mock<IFeatureFlagEvaluator>().Object,
            new FeatureFlagOptions());

        await Assert.ThrowsAsync<ArgumentException>(() => sut.IsEnabledAsync(string.Empty));
    }

    [Fact]
    public async Task IsEnabledAsync_NullContext_ThrowsArgumentNullException()
    {
        var sut = new FeatureFlagService(
            new Mock<IFeatureFlagProvider>().Object,
            new Mock<IFeatureFlagEvaluator>().Object,
            new FeatureFlagOptions());

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.IsEnabledAsync("key", (EvaluationContext)null!));
    }

    [Fact]
    public async Task IsEnabledAsync_NullConfigureAction_ThrowsArgumentNullException()
    {
        var sut = new FeatureFlagService(
            new Mock<IFeatureFlagProvider>().Object,
            new Mock<IFeatureFlagEvaluator>().Object,
            new FeatureFlagOptions());

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.IsEnabledAsync("key", (Action<EvaluationContextBuilder>)null!));
    }
}