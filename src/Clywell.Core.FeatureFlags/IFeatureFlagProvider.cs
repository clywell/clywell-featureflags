namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Source of truth for feature flag definitions.
/// Consumers must register exactly one implementation in DI.
/// The provider owns caching concerns — the evaluation engine calls it on every evaluation.
/// </summary>
public interface IFeatureFlagProvider
{
    /// <summary>Returns all known feature flags.</summary>
    Task<IReadOnlyList<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the flag with the given <paramref name="key"/>, or <see langword="null"/> if it does not exist.
    /// </summary>
    Task<FeatureFlag?> GetAsync(string key, CancellationToken cancellationToken = default);
}
