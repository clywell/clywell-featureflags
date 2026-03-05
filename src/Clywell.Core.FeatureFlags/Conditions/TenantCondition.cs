using System.Collections.Frozen;

namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// An <see cref="IEvaluationCondition"/> that matches when the context's tenant identifier
/// is in the configured set of allowed tenant IDs.
/// </summary>
public sealed class TenantCondition : IEvaluationCondition
{
    private readonly IReadOnlySet<string> _tenantIds;

    /// <param name="tenantIds">
    /// One or more tenant IDs for which this condition is satisfied.
    /// Pass a single ID or multiple IDs - all are matched case-insensitively.
    /// </param>
    public TenantCondition(params IEnumerable<string> tenantIds)
    {
        ArgumentNullException.ThrowIfNull(tenantIds);
        _tenantIds = tenantIds.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public bool Matches(EvaluationContext context) =>
        context.TenantId is not null && _tenantIds.Contains(context.TenantId);
}
