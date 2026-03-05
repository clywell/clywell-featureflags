namespace Clywell.Core.FeatureFlags.Builders;

/// <summary>
/// Fluent builder for constructing <see cref="FeatureFlag"/> instances.
/// </summary>
/// <remarks>
/// Typical provider usage:
/// <code>
/// return FeatureFlagBuilder
///     .For("new-checkout")
///     .WithDescription("Gradual rollout of the redesigned checkout flow")
///     .DisabledByDefault()
///     .EnableWhen(new TenantCondition(["tenant-early-access"]))
///     .EnableWhen(new AttributeCondition("plan", "enterprise"))
///     .Build();
/// </code>
/// Rules added first have the highest auto-assigned priority. Pass an explicit priority value to override.
/// </remarks>
public sealed class FeatureFlagBuilder
{
    private readonly string _key;
    private string? _description;
    private bool _defaultValue;
    private readonly List<(IEvaluationCondition Condition, bool Value, int? ExplicitPriority)> _rules = [];

    private FeatureFlagBuilder(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _key = key;
    }

    /// <summary>Creates a new builder for the flag with the given <paramref name="key"/>.</summary>
    public static FeatureFlagBuilder For(string key) => new(key);

    /// <summary>Sets the human-readable description for this flag.</summary>
    public FeatureFlagBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>Sets <see cref="FeatureFlag.DefaultValue"/> to <see langword="true"/> - flag is on unless a rule disables it.</summary>
    public FeatureFlagBuilder EnabledByDefault()
    {
        _defaultValue = true;
        return this;
    }

    /// <summary>Sets <see cref="FeatureFlag.DefaultValue"/> to <see langword="false"/> - flag is off unless a rule enables it. This is the default.</summary>
    public FeatureFlagBuilder DisabledByDefault()
    {
        _defaultValue = false;
        return this;
    }

    /// <summary>
    /// Adds a rule that enables the flag when <paramref name="condition"/> matches.
    /// Rules added earlier have higher auto-assigned priority.
    /// </summary>
    /// <param name="condition">The condition that must match for the flag to be enabled.</param>
    /// <param name="priority">Explicit priority; overrides auto-assignment when provided.</param>
    public FeatureFlagBuilder EnableWhen(IEvaluationCondition condition, int? priority = null)
        => AddRule(condition, true, priority);

    /// <summary>
    /// Adds a rule that disables the flag when <paramref name="condition"/> matches.
    /// Useful for kill switches layered over a broadly-enabled flag.
    /// Rules added earlier have higher auto-assigned priority.
    /// </summary>
    /// <param name="condition">The condition that must match for the flag to be disabled.</param>
    /// <param name="priority">Explicit priority; overrides auto-assignment when provided.</param>
    public FeatureFlagBuilder DisableWhen(IEvaluationCondition condition, int? priority = null)
        => AddRule(condition, false, priority);

    /// <summary>Constructs an immutable <see cref="FeatureFlag"/> from the current builder state.</summary>
    public FeatureFlag Build()
    {
        var totalRules = _rules.Count;
        var rules = _rules
            .Select((r, index) => new EvaluationRule
            {
                Priority = r.ExplicitPriority ?? (totalRules - index) * 10,
                Condition = r.Condition,
                Value = r.Value
            })
            .ToList();

        return new FeatureFlag
        {
            Key = _key,
            Description = _description,
            DefaultValue = _defaultValue,
            Rules = rules
        };
    }

    private FeatureFlagBuilder AddRule(IEvaluationCondition condition, bool value, int? priority)
    {
        ArgumentNullException.ThrowIfNull(condition);
        _rules.Add((condition, value, priority));
        return this;
    }
}
