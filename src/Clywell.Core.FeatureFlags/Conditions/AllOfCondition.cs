namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// An <see cref="IEvaluationCondition"/> that matches only when every inner condition matches (logical AND).
/// Returns <see langword="true"/> vacuously when the condition set is empty.
/// </summary>
public sealed class AllOfCondition : IEvaluationCondition
{
    private readonly IReadOnlyList<IEvaluationCondition> _conditions;

    /// <param name="conditions">The conditions that must all be satisfied.</param>
    public AllOfCondition(IEnumerable<IEvaluationCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        _conditions = [.. conditions];
    }

    /// <inheritdoc/>
    public bool Matches(EvaluationContext context) =>
        _conditions.All(c => c.Matches(context));
}
