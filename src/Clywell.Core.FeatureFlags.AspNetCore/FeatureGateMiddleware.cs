namespace Clywell.Core.FeatureFlags.AspNetCore;

/// <summary>
/// Middleware that gates an entire path segment behind a feature flag.
/// Register via <see cref="ApplicationBuilderExtensions.UseFeatureGate"/>.
/// </summary>
/// <remarks>
/// The flag is evaluated with <see cref="EvaluationContext.Empty"/> (no tenant, no user, no attributes).
/// Rules using <c>TenantCondition</c>, <c>UserCondition</c>, or <c>PercentageCondition</c> will not match here.
/// Register via <see cref="ApplicationBuilderExtensions.UseFeatureGate"/>.
/// </remarks>
internal sealed class FeatureGateMiddleware(RequestDelegate next, string flagKey, FeatureGateOptions options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var service = context.RequestServices.GetRequiredService<IFeatureFlagService>();
        var isEnabled = await service
            .IsEnabledAsync(flagKey, context.RequestAborted)
            .ConfigureAwait(false);

        if (!isEnabled)
        {
            if (!string.IsNullOrEmpty(options.DisabledRedirectPath))
            {
                context.Response.Redirect(options.DisabledRedirectPath);
                return;
            }

            context.Response.StatusCode = options.DisabledStatusCode;
            return;
        }

        await next(context).ConfigureAwait(false);
    }
}
