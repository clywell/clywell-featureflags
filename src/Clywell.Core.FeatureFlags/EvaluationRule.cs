namespace Clywell.Core.FeatureFlags;

/// <summary>
/// A single rule within a <see cref="FeatureFlag"/>. When <see cref="Condition"/> matches the context,
/// the flag resolves to <see cref="Value"/>.
/// </summary>
public sealed record EvaluationRule
{
    /// <summary>Evaluation order; higher values are evaluated first.</summary>
    public required int Priority { get; init; }

    /// <summary>The condition that must be satisfied for this rule to fire.</summary>
    public required IEvaluationCondition Condition { get; init; }

    /// <summary>The resolved value when this rule's condition matches.</summary>
    public required bool Value { get; init; }
}
