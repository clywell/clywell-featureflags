namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Extension methods for registering feature flag services into the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core feature flag evaluation engine.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="FeatureFlagOptions"/>.
    /// After calling this, register an <see cref="IFeatureFlagProvider"/> implementation:
    /// <code>
    /// services.AddFeatureFlags();
    /// services.AddScoped&lt;IFeatureFlagProvider, MyProvider&gt;();
    /// </code>
    /// </param>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        Action<FeatureFlagOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new FeatureFlagOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.TryAddSingleton<IFeatureFlagEvaluator, DefaultFeatureFlagEvaluator>();
        services.TryAddScoped<IFeatureFlagService, FeatureFlagService>();

        return services;
    }
}
