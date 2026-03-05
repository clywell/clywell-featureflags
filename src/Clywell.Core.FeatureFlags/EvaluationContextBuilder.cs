using System.Collections.Frozen;

namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Fluent builder for constructing an immutable <see cref="EvaluationContext"/>.
/// </summary>
public sealed class EvaluationContextBuilder
{
    private string? _tenantId;
    private string? _userId;
    private readonly Dictionary<string, string> _attributes = [];

    /// <summary>Sets the tenant identifier for this evaluation.</summary>
    public EvaluationContextBuilder WithTenant(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        _tenantId = tenantId;
        return this;
    }

    /// <summary>Sets the user identifier for this evaluation.</summary>
    public EvaluationContextBuilder WithUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        _userId = userId;
        return this;
    }

    /// <summary>Adds or overwrites a custom attribute. Last write wins for duplicate keys.</summary>
    public EvaluationContextBuilder WithAttribute(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _attributes[key] = value;
        return this;
    }

    /// <summary>Constructs an immutable <see cref="EvaluationContext"/> from the current builder state.</summary>
    public EvaluationContext Build() =>
        new()
        {
            TenantId = _tenantId,
            UserId = _userId,
            Attributes = _attributes.Count == 0
                ? FrozenDictionary<string, string>.Empty
                : _attributes.ToFrozenDictionary()
        };
}
