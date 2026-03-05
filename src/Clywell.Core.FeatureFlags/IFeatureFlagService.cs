namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Primary API for evaluating feature flags. Inject and call <c>IsEnabledAsync</c>
/// at any call site — CQRS handlers, domain services, API controllers, middleware.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Returns whether the flag identified by <paramref name="key"/> is enabled
    /// with an empty evaluation context (no tenant, no user, no attributes).
    /// </summary>
    Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the flag is enabled for the given pre-built <paramref name="context"/>.
    /// </summary>
    Task<bool> IsEnabledAsync(string key, EvaluationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the flag is enabled after building the context with <paramref name="configure"/>.
    /// </summary>
    Task<bool> IsEnabledAsync(string key, Action<EvaluationContextBuilder> configure, CancellationToken cancellationToken = default);
}
