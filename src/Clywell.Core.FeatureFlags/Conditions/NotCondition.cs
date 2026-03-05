namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// An <see cref="IEvaluationCondition"/> that inverts the result of an inner condition (logical NOT).
/// </summary>
public sealed class NotCondition : IEvaluationCondition
{
    private readonly IEvaluationCondition _inner;

    /// <param name="inner">The condition whose result will be negated.</param>
    public NotCondition(IEvaluationCondition inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
    }

    /// <inheritdoc/>
    public bool Matches(EvaluationContext context) => !_inner.Matches(context);
}
