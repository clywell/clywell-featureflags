namespace Clywell.Core.FeatureFlags.AspNetCore;

/// <summary>
/// MVC action filter that evaluates a feature flag before executing a controller action.
/// When the flag is disabled the filter short-circuits with a gate response.
/// </summary>
/// <remarks>
/// The attribute resolves <see cref="IFeatureFlagService"/> and <see cref="FeatureGateOptions"/>
/// from the request's service provider at execution time. Both must be registered in DI.
/// <para>
/// <strong>Minimal API note:</strong> This attribute has no effect on Minimal API route handlers.
/// For Minimal APIs use the <c>.RequireFeature("flag-key")</c> extension on the route builder instead:
/// <code>app.MapPost("/route", handler).RequireFeature("flag-key");</code>
/// </para>
/// <para>
/// <strong>Context note:</strong> The flag is evaluated with <see cref="EvaluationContext.Empty"/> (no tenant, no user, no attributes).
/// Rules using <c>TenantCondition</c>, <c>UserCondition</c>, or <c>PercentageCondition</c> will always evaluate to <see langword="false"/> here.
/// To evaluate with a populated context, inject <see cref="IFeatureFlagService"/> directly into your controller or handler.
/// </para>
/// </remarks>
/// <param name="flagKey">The feature flag key to evaluate on every request.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequiresFeatureAttribute(string flagKey) : ActionFilterAttribute
{
    /// <inheritdoc/>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var service = services.GetRequiredService<IFeatureFlagService>();
        var options = services.GetService<FeatureGateOptions>() ?? new FeatureGateOptions();

        var isEnabled = await service
            .IsEnabledAsync(flagKey, context.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (!isEnabled)
        {
            context.Result = string.IsNullOrEmpty(options.DisabledRedirectPath)
                ? new StatusCodeResult(options.DisabledStatusCode)
                : new RedirectResult(options.DisabledRedirectPath);
            return;
        }

        await next().ConfigureAwait(false);
    }
}
