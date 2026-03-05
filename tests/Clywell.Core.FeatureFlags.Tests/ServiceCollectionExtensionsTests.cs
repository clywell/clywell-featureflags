namespace Clywell.Core.FeatureFlags.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFeatureFlags_RegistersFeatureFlagServiceAsScoped()
    {
        var services = new ServiceCollection();
        services.AddScoped<IFeatureFlagProvider>(_ => new Mock<IFeatureFlagProvider>().Object);

        services.AddFeatureFlags();

        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(IFeatureFlagService));

        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddFeatureFlags_RegistersFeatureFlagEvaluatorAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddFeatureFlags();

        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(IFeatureFlagEvaluator));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddFeatureFlags_RegistersFeatureFlagOptionsAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddFeatureFlags();

        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(FeatureFlagOptions));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddFeatureFlags_WithConfigureDelegate_SetsDefaultValueWhenNotFound()
    {
        var services = new ServiceCollection();
        services.AddScoped<IFeatureFlagProvider>(_ => new Mock<IFeatureFlagProvider>().Object);

        services.AddFeatureFlags(options => options.WithDefaultValueWhenNotFound(true));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<FeatureFlagOptions>();

        Assert.True(options.DefaultValueWhenNotFound);
    }

    [Fact]
    public void AddFeatureFlags_CalledTwice_RegistersSingleFeatureFlagService()
    {
        var services = new ServiceCollection();

        services.AddFeatureFlags();
        services.AddFeatureFlags();

        var serviceRegistrations = services.Count(d => d.ServiceType == typeof(IFeatureFlagService));
        Assert.Equal(1, serviceRegistrations);
    }

    [Fact]
    public void AddFeatureFlags_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddFeatureFlags(null!));
    }
}