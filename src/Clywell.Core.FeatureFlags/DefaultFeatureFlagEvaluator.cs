namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Default rule evaluation strategy. Sorts rules by <see cref="EvaluationRule.Priority"/> descending
/// and returns the value of the first rule whose condition matches the context.
/// Falls back to <see cref="FeatureFlag.DefaultValue"/> when no rule matches.
/// Rules are sorted by <see cref="EvaluationRule.Priority"/> on each evaluation. For optimal performance,
/// providers should return rules in descending priority order to avoid unnecessary allocations.
/// </summary>
internal sealed class DefaultFeatureFlagEvaluator : IFeatureFlagEvaluator
{
    /// <inheritdoc/>
    public bool Evaluate(FeatureFlag flag, EvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(flag);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var rule in flag.Rules.OrderByDescending(r => r.Priority))
        {
            if (rule.Condition.Matches(context))
                return rule.Value;
        }

        return flag.DefaultValue;
    }
}
