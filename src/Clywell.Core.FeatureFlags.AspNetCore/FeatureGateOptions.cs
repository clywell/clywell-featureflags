namespace Clywell.Core.FeatureFlags.AspNetCore;

/// <summary>
/// Options governing how HTTP feature gates behave when a feature flag is disabled.
/// </summary>
public sealed class FeatureGateOptions
{
    /// <summary>
    /// HTTP status code returned when a feature is disabled and no redirect is configured.
    /// Defaults to 404 (Not Found) — disabled features appear non-existent to callers.
    /// </summary>
    public int DisabledStatusCode { get; set; } = StatusCodes.Status404NotFound;

    /// <summary>
    /// When set, disabled gates redirect to this path instead of returning <see cref="DisabledStatusCode"/>.
    /// Must be a relative or absolute URL. <see langword="null"/> (default) means no redirect.
    /// </summary>
    public string? DisabledRedirectPath { get; set; }
}
