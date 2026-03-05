namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// Static factory and extension methods for composing <see cref="IEvaluationCondition"/> instances fluently.
/// </summary>
/// <example>
/// <code>
/// // AND - require enterprise plan AND a specific tenant:
/// Condition.AllOf(
///     new AttributeCondition("plan", "enterprise"),
///     new TenantCondition(["tenant-a"])
/// )
///
/// // OR - allow early-access tenants OR beta users:
/// Condition.AnyOf(
///     new TenantCondition(["early-access-tenant"]),
///     new UserCondition(["beta-user-1"])
/// )
///
/// // Extension method chaining:
/// new AttributeCondition("plan", "enterprise")
///     .And(new TenantCondition(["tenant-a"]))
///     .Or(new UserCondition(["admin"]))
/// </code>
/// </example>
public static class Condition
{
    /// <summary>Returns a condition that matches only when all <paramref name="conditions"/> match (AND).</summary>
    public static IEvaluationCondition AllOf(params IEvaluationCondition[] conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        return new AllOfCondition(conditions);
    }

    /// <summary>Returns a condition that matches when at least one of <paramref name="conditions"/> matches (OR).</summary>
    public static IEvaluationCondition AnyOf(params IEvaluationCondition[] conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        return new AnyOfCondition(conditions);
    }

    /// <summary>Returns a condition that matches when <paramref name="inner"/> does NOT match (NOT).</summary>
    public static IEvaluationCondition Not(IEvaluationCondition inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        return new NotCondition(inner);
    }

    /// <summary>Combines this condition with <paramref name="right"/> using logical AND.</summary>
    public static IEvaluationCondition And(this IEvaluationCondition left, IEvaluationCondition right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return AllOf(left, right);
    }

    /// <summary>Combines this condition with <paramref name="right"/> using logical OR.</summary>
    public static IEvaluationCondition Or(this IEvaluationCondition left, IEvaluationCondition right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return AnyOf(left, right);
    }

    /// <summary>Returns the logical NOT of this condition.</summary>
    public static IEvaluationCondition Negate(this IEvaluationCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        return Not(condition);
    }
}
