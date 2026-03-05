namespace Clywell.Core.FeatureFlags;

/// <summary>
/// An evaluation condition tested against an <see cref="EvaluationContext"/>.
/// Implement this interface to create custom targeting rules.
/// </summary>
public interface IEvaluationCondition
{
    /// <summary>
    /// Returns <see langword="true"/> when this condition is satisfied by the given context.
    /// </summary>
    bool Matches(EvaluationContext context);
}
