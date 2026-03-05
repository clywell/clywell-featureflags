namespace Clywell.Core.FeatureFlags.Conditions;

/// <summary>
/// An <see cref="IEvaluationCondition"/> that enables a feature for a consistent percentage
/// of the audience using a deterministic FNV-1a hash.
/// The same user (or tenant) always falls in the same bucket for the same flag key.
/// </summary>
public sealed class PercentageCondition : IEvaluationCondition
{
    private readonly string _flagKey;
    private readonly int _percentage;

    /// <param name="flagKey">
    /// The flag key, included in the hash seed so different flags produce independent audience splits.
    /// </param>
    /// <param name="percentage">Target percentage of the audience (0-100 inclusive).</param>
    public PercentageCondition(string flagKey, int percentage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flagKey);
        ArgumentOutOfRangeException.ThrowIfNegative(percentage);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percentage, 100);
        _flagKey = flagKey;
        _percentage = percentage;
    }

    /// <inheritdoc/>
    public bool Matches(EvaluationContext context)
    {
        if (_percentage == 0) return false;
        if (_percentage == 100) return true;

        var discriminator = context.UserId ?? context.TenantId;
        if (discriminator is null) return false;

        var bucket = Fnv1aHash($"{_flagKey}:{discriminator}") % 100;
        return bucket < (uint)_percentage;
    }

    /// <summary>
    /// FNV-1a 32-bit deterministic hash.
    /// Unlike <see cref="System.HashCode"/>, this produces the same result across processes and runtimes.
    /// Input is treated as Latin-1 / ASCII; characters above U+00FF are hashed by their low byte only.
    /// Tenant and user IDs using standard UUID, alphanumeric, or base64 formats are unaffected.
    /// </summary>
    private static uint Fnv1aHash(string value)
    {
        const uint fnvOffset = 2166136261u;
        const uint fnvPrime = 16777619u;
        var hash = fnvOffset;
        foreach (var c in value)
        {
            hash ^= (byte)c;
            hash *= fnvPrime;
        }
        return hash;
    }
}
