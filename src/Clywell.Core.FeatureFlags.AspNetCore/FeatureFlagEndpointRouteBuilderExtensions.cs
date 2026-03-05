namespace Clywell.Core.FeatureFlags.AspNetCore;

/// <summary>
/// Extension methods for adding feature flag gates to Minimal API route handlers.
/// </summary>
public static class FeatureFlagEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="FeatureFlagEndpointFilter"/> to the route handler.
    /// When the flag is disabled, the endpoint returns a gate response without invoking the handler.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="flagKey">The feature flag key to evaluate on every request to this endpoint.</param>
    public static TBuilder RequireFeature<TBuilder>(this TBuilder builder, string flagKey)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(flagKey);

        return builder.AddEndpointFilter(new FeatureFlagEndpointFilter(flagKey));
    }
}
