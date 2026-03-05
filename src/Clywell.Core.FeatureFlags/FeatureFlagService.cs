namespace Clywell.Core.FeatureFlags;

/// <summary>
/// Default implementation of <see cref="IFeatureFlagService"/>.
/// Resolves flag definitions from <see cref="IFeatureFlagProvider"/> on every call.
/// Caching should be implemented inside the provider — not here.
/// </summary>
internal sealed class FeatureFlagService(
    IFeatureFlagProvider provider,
    IFeatureFlagEvaluator evaluator,
    FeatureFlagOptions options) : IFeatureFlagService
{

    /// <inheritdoc/>
#pragma warning disable CA1822
    public Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default) =>
        IsEnabledAsync(key, EvaluationContext.Empty, cancellationToken);
#pragma warning restore CA1822

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(
        string key,
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(context);

        var flag = await provider.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (flag is null)
            return options.DefaultValueWhenNotFound;

        return evaluator.Evaluate(flag, context);
    }

    /// <inheritdoc/>
    public Task<bool> IsEnabledAsync(
        string key,
        Action<EvaluationContextBuilder> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new EvaluationContextBuilder();
        configure(builder);
        return IsEnabledAsync(key, builder.Build(), cancellationToken);
    }
}
