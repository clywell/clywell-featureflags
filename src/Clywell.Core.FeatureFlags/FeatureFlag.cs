namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Represents a feature flag definition returned by an <see cref="IFeatureFlagProvider"/>.
/// </summary>
public sealed record FeatureFlag
{
    /// <summary>Unique, stable identifier for this flag (e.g. <c>"new-checkout"</c>).</summary>
    public required string Key { get; init; }

    /// <summary>Human-readable description shown in admin UIs or logs.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Fallback value used when no <see cref="Rules"/> condition matches the evaluation context,
    /// or when this flag is not found by the provider.
    /// </summary>
    public bool DefaultValue { get; init; }

    /// <summary>Ordered evaluation rules. May be empty.</summary>
    public IReadOnlyList<EvaluationRule> Rules { get; init; } = [];
}
