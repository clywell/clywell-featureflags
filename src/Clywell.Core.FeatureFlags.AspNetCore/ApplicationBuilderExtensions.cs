namespace Clywell.Core.FeatureFlags.AspNetCore;

/// <summary>
/// Extension methods for adding feature flag gate middleware to the request pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds a middleware gate that blocks requests when the given feature flag is disabled.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="key">The feature flag key to evaluate on every request.</param>
    /// <param name="disabledPath">
    /// When set, disabled requests are redirected here, overriding
    /// <see cref="FeatureGateOptions.DisabledRedirectPath"/> from DI.
    /// </param>
    public static IApplicationBuilder UseFeatureGate(
        this IApplicationBuilder app,
        string key,
        string? disabledPath = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var baseOptions = app.ApplicationServices.GetService<FeatureGateOptions>() ?? new FeatureGateOptions();

        var options = disabledPath is not null
            ? new FeatureGateOptions
            {
                DisabledStatusCode = baseOptions.DisabledStatusCode,
                DisabledRedirectPath = disabledPath
            }
            : baseOptions;

        return app.UseMiddleware<FeatureGateMiddleware>(key, options);
    }
}
