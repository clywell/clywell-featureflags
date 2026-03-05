namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// An <see cref="IEvaluationCondition"/> that matches when at least one inner condition matches (logical OR).
/// Returns <see langword="false"/> vacuously when the condition set is empty.
/// </summary>
public sealed class AnyOfCondition : IEvaluationCondition
{
    private readonly IReadOnlyList<IEvaluationCondition> _conditions;

    /// <param name="conditions">The conditions, at least one of which must be satisfied.</param>
    public AnyOfCondition(IEnumerable<IEvaluationCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        _conditions = [.. conditions];
    }

    /// <inheritdoc/>
    public bool Matches(EvaluationContext context) =>
        _conditions.Any(c => c.Matches(context));
}
