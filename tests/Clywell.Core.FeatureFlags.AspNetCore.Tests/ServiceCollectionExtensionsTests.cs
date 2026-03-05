namespace Clywell.Core.FeatureFlags.AspNetCore.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFeatureFlagsAspNetCore_WhenCalled_RegistersFeatureGateOptionsSingleton()
    {
        var services = new ServiceCollection();

        services.AddFeatureFlagsAspNetCore();

        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(FeatureGateOptions));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddFeatureFlagsAspNetCore_WithConfigureDelegate_SetsDisabledStatusCode()
    {
        var services = new ServiceCollection();

        services.AddFeatureFlagsAspNetCore(options => options.DisabledStatusCode = StatusCodes.Status403Forbidden);
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<FeatureGateOptions>();
        Assert.Equal(StatusCodes.Status403Forbidden, options.DisabledStatusCode);
    }

    [Fact]
    public void AddFeatureFlagsAspNetCore_WithConfigureDelegate_SetsDisabledRedirectPath()
    {
        var services = new ServiceCollection();

        services.AddFeatureFlagsAspNetCore(options => options.DisabledRedirectPath = "/feature-disabled");
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<FeatureGateOptions>();
        Assert.Equal("/feature-disabled", options.DisabledRedirectPath);
    }

    [Fact]
    public void AddFeatureFlagsAspNetCore_WhenCalledTwice_KeepsFirstRegistration()
    {
        var services = new ServiceCollection();

        services.AddFeatureFlagsAspNetCore(options => options.DisabledStatusCode = StatusCodes.Status403Forbidden);
        services.AddFeatureFlagsAspNetCore(options => options.DisabledStatusCode = StatusCodes.Status418ImATeapot);
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<FeatureGateOptions>();
        Assert.Equal(StatusCodes.Status403Forbidden, options.DisabledStatusCode);
        Assert.Single(
            services,
            d => d.ServiceType == typeof(FeatureGateOptions));
    }

    [Fact]
    public void AddFeatureFlagsAspNetCore_WhenServicesIsNull_ThrowsArgumentNullException()
    {
        ServiceCollection? services = null;

        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddFeatureFlagsAspNetCore(services!));
    }
}