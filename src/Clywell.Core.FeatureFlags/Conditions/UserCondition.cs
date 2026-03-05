using System.Collections.Frozen;

namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// An <see cref="IEvaluationCondition"/> that matches when the context's user identifier
/// is in the configured set of allowed user IDs.
/// </summary>
public sealed class UserCondition : IEvaluationCondition
{
    private readonly IReadOnlySet<string> _userIds;

    /// <param name="userIds">
    /// One or more user IDs for which this condition is satisfied.
    /// Pass a single ID or multiple IDs - all are matched case-insensitively.
    /// </param>
    public UserCondition(params IEnumerable<string> userIds)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        _userIds = userIds.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public bool Matches(EvaluationContext context) =>
        context.UserId is not null && _userIds.Contains(context.UserId);
}
