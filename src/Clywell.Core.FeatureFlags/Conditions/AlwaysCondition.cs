namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// An <see cref="IEvaluationCondition"/> that always returns <see langword="true"/>,
/// regardless of the evaluation context. Use as a lowest-priority catch-all rule.
/// </summary>
public sealed class AlwaysCondition : IEvaluationCondition
{
    /// <summary>A shared, reusable instance.</summary>
    public static readonly AlwaysCondition Instance = new();

    /// <inheritdoc/>
#pragma warning disable CA1822
    public bool Matches(EvaluationContext context) => true;
#pragma warning restore CA1822

}
