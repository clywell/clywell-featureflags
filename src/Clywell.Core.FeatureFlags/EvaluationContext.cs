using System.Collections.Frozen;

namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Carries contextual data for a single flag evaluation.
/// All properties are optional; pass <see cref="Empty"/> when no context is available.
/// </summary>
public sealed record EvaluationContext
{
    /// <summary>An <see cref="EvaluationContext"/> with no tenant, user, or attributes set.</summary>
    public static readonly EvaluationContext Empty = new();

    /// <summary>Identifier of the current tenant, or <see langword="null"/> if not in a tenanted request.</summary>
    public string? TenantId { get; init; }

    /// <summary>Identifier of the current user, or <see langword="null"/> if anonymous.</summary>
    public string? UserId { get; init; }

    /// <summary>Arbitrary key/value attributes for custom rule matching (e.g. "plan" = "enterprise").</summary>
    public IReadOnlyDictionary<string, string> Attributes { get; init; }
        = FrozenDictionary<string, string>.Empty;
}
