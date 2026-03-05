using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Clywell.Core.FeatureFlags.AspNetCore.Tests;

public sealed class RequiresFeatureAttributeTests
{
    [Fact]
    public async Task OnActionExecutionAsync_FlagEnabled_CallsNextAndLeavesResultNull()
    {
        var context = CreateActionExecutingContext(isEnabled: true, options: new FeatureGateOptions());
        var attribute = new RequiresFeatureAttribute("orders");

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            var actionContext = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor, context.ModelState);
            return Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));
        };

        await attribute.OnActionExecutionAsync(context, next);

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_FlagDisabledWithoutRedirect_SetsStatusCodeResult404()
    {
        var context = CreateActionExecutingContext(isEnabled: false, options: new FeatureGateOptions());
        var attribute = new RequiresFeatureAttribute("orders");

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            var actionContext = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor, context.ModelState);
            return Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));
        };

        await attribute.OnActionExecutionAsync(context, next);

        var statusCodeResult = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task OnActionExecutionAsync_FlagDisabledWithCustomStatusCode_SetsConfiguredStatusCodeResult()
    {
        var options = new FeatureGateOptions { DisabledStatusCode = StatusCodes.Status403Forbidden };
        var context = CreateActionExecutingContext(isEnabled: false, options: options);
        var attribute = new RequiresFeatureAttribute("orders");

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            var actionContext = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor, context.ModelState);
            return Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));
        };

        await attribute.OnActionExecutionAsync(context, next);

        var statusCodeResult = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusCodeResult.StatusCode);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task OnActionExecutionAsync_FlagDisabledWithRedirect_SetsRedirectResult()
    {
        var options = new FeatureGateOptions { DisabledRedirectPath = "/feature-disabled" };
        var context = CreateActionExecutingContext(isEnabled: false, options: options);
        var attribute = new RequiresFeatureAttribute("orders");

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            var actionContext = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor, context.ModelState);
            return Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));
        };

        await attribute.OnActionExecutionAsync(context, next);

        var redirectResult = Assert.IsType<RedirectResult>(context.Result);
        Assert.Equal("/feature-disabled", redirectResult.Url);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenFeatureFlagServiceIsMissing_ThrowsInvalidOperationException()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(new FeatureGateOptions())
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            new object());

        var attribute = new RequiresFeatureAttribute("orders");
        ActionExecutionDelegate next = () =>
            Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => attribute.OnActionExecutionAsync(context, next));
    }

    private static ActionExecutingContext CreateActionExecutingContext(bool isEnabled, FeatureGateOptions options)
    {
        var featureFlagService = new Mock<IFeatureFlagService>();
        featureFlagService
            .Setup(s => s.IsEnabledAsync("orders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(isEnabled);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(featureFlagService.Object)
            .AddSingleton(options)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            new object());
    }
}