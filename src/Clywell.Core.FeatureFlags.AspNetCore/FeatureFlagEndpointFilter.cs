namespace Clywell.Core.FeatureFlags.AspNetCore;

/// <summary>
/// Minimal API endpoint filter that gates a route handler behind a feature flag.
/// Add via <see cref="FeatureFlagEndpointRouteBuilderExtensions.RequireFeature{TBuilder}"/>.
/// </summary>
/// <remarks>
/// The flag is evaluated with <see cref="EvaluationContext.Empty"/> (no tenant, no user, no attributes).
/// Rules using <c>TenantCondition</c>, <c>UserCondition</c>, or <c>PercentageCondition</c> will not match here.
/// Add via <see cref="FeatureFlagEndpointRouteBuilderExtensions.RequireFeature{TBuilder}"/>.
/// </remarks>
internal sealed class FeatureFlagEndpointFilter(string flagKey) : IEndpointFilter
{
    /// <inheritdoc/>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var service = services.GetRequiredService<IFeatureFlagService>();
        var options = services.GetService<FeatureGateOptions>() ?? new FeatureGateOptions();

        var isEnabled = await service
            .IsEnabledAsync(flagKey, context.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (!isEnabled)
        {
            return string.IsNullOrEmpty(options.DisabledRedirectPath)
                ? Results.StatusCode(options.DisabledStatusCode)
                : Results.Redirect(options.DisabledRedirectPath);
        }

        return await next(context).ConfigureAwait(false);
    }
}
