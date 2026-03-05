namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Evaluates a <see cref="FeatureFlag"/> against an <see cref="EvaluationContext"/> and returns
/// the resolved boolean value.
/// Register a custom implementation to replace the default priority-order evaluator.
/// </summary>
public interface IFeatureFlagEvaluator
{
    /// <summary>
    /// Evaluates the flag rules in priority order and returns the first matching value,
    /// or <see cref="FeatureFlag.DefaultValue"/> if no rule matches.
    /// </summary>
    bool Evaluate(FeatureFlag flag, EvaluationContext context);
}
