namespace Clywell.Core.FeatureFlags.AspNetCore;

/// <summary>
/// Extension methods for registering ASP.NET Core feature flag gate services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="FeatureGateOptions"/> as a singleton and makes it available to
    /// <see cref="RequiresFeatureAttribute"/>, <see cref="FeatureGateMiddleware"/>, and
    /// <see cref="FeatureFlagEndpointFilter"/>.
    /// Call after <c>services.AddFeatureFlags()</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to customise gate behaviour.</param>
    public static IServiceCollection AddFeatureFlagsAspNetCore(
        this IServiceCollection services,
        Action<FeatureGateOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new FeatureGateOptions();
        configure?.Invoke(options);
        services.TryAddSingleton(options);

        return services;
    }
}
