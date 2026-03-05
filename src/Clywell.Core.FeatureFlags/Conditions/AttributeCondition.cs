namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// An <see cref="IEvaluationCondition"/> that matches when a specific attribute
/// in the evaluation context equals the expected value.
/// </summary>
public sealed class AttributeCondition : IEvaluationCondition
{
    private readonly string _key;
    private readonly string _value;
    private readonly StringComparison _comparison;

    /// <param name="key">The attribute key to inspect.</param>
    /// <param name="value">The required attribute value.</param>
    /// <param name="comparison">String comparison mode; defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.</param>
    public AttributeCondition(
        string key,
        string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _key = key;
        _value = value;
        _comparison = comparison;
    }

    /// <inheritdoc/>
    public bool Matches(EvaluationContext context) =>
        context.Attributes.TryGetValue(_key, out var actual) &&
        string.Equals(actual, _value, _comparison);
}
