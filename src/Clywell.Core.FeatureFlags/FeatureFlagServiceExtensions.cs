namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Convenience extension methods for <see cref="IFeatureFlagService"/>.
/// </summary>
public static class FeatureFlagServiceExtensions
{
    /// <summary>
    /// Returns whether the flag is enabled in the context of the given <paramref name="tenantId"/>.
    /// Shorthand for <c>IsEnabledAsync(key, ctx => ctx.WithTenant(tenantId))</c>.
    /// </summary>
    public static Task<bool> IsEnabledAsync(
        this IFeatureFlagService service,
        string key,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        return service.IsEnabledAsync(
            key,
            ctx => ctx.WithTenant(tenantId),
            cancellationToken);
    }

    /// <summary>
    /// Returns whether the flag is enabled in the context of the given tenant and user.
    /// Shorthand for <c>IsEnabledAsync(key, ctx => ctx.WithTenant(tenantId).WithUser(userId))</c>.
    /// </summary>
    public static Task<bool> IsEnabledAsync(
        this IFeatureFlagService service,
        string key,
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return service.IsEnabledAsync(
            key,
            ctx => ctx.WithTenant(tenantId).WithUser(userId),
            cancellationToken);
    }

    /// <summary>
    /// Evaluates multiple flags against the same <paramref name="context"/> concurrently and returns
    /// a dictionary mapping each key to its resolved value.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, bool>> EvaluateManyAsync(
        this IFeatureFlagService service,
        IEnumerable<string> keys,
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(context);

        var keyList = keys.ToList();
        var results = await Task.WhenAll(
            keyList.Select(k => service.IsEnabledAsync(k, context, cancellationToken)))
            .ConfigureAwait(false);

        return keyList
            .Zip(results, (k, v) => (k, v))
            .ToDictionary(pair => pair.k, pair => pair.v);
    }

    /// <summary>
    /// Evaluates multiple flags after building the context with <paramref name="configure"/> and returns
    /// a dictionary mapping each key to its resolved value.
    /// Flags are evaluated concurrently.
    /// </summary>
    public static Task<IReadOnlyDictionary<string, bool>> EvaluateManyAsync(
        this IFeatureFlagService service,
        IEnumerable<string> keys,
        Action<EvaluationContextBuilder> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new EvaluationContextBuilder();
        configure(builder);
        return service.EvaluateManyAsync(keys, builder.Build(), cancellationToken);
    }
}
