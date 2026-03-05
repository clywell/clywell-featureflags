namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Configuration for the feature flag evaluation engine.
/// </summary>
public sealed class FeatureFlagOptions
{
    /// <summary>
    /// Value returned when a requested flag key does not exist in the provider.
    /// Defaults to <see langword="false"/> (safe-off — unknown flags are disabled).
    /// </summary>
    public bool DefaultValueWhenNotFound { get; private set; }

    /// <summary>
    /// Sets the fallback value returned when a flag key is not found via <see cref="IFeatureFlagProvider"/>.
    /// </summary>
    public FeatureFlagOptions WithDefaultValueWhenNotFound(bool value)
    {
        DefaultValueWhenNotFound = value;
        return this;
    }
}
