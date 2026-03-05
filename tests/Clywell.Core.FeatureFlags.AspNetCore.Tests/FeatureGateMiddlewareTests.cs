namespace Clywell.Core.FeatureFlags.AspNetCore.Tests;

public sealed class FeatureGateMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_FlagEnabled_CallsNextDelegate()
    {
        var context = new DefaultHttpContext();
        var featureFlagService = new Mock<IFeatureFlagService>();
        featureFlagService
            .Setup(s => s.IsEnabledAsync("orders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var services = new ServiceCollection()
            .AddSingleton(featureFlagService.Object)
            .BuildServiceProvider();
        context.RequestServices = services;

        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new FeatureGateMiddleware(next, "orders", new FeatureGateOptions());

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_FlagDisabledWithoutRedirect_DoesNotCallNextAndReturns404()
    {
        var context = new DefaultHttpContext();
        var featureFlagService = new Mock<IFeatureFlagService>();
        featureFlagService
            .Setup(s => s.IsEnabledAsync("orders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var services = new ServiceCollection()
            .AddSingleton(featureFlagService.Object)
            .BuildServiceProvider();
        context.RequestServices = services;

        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new FeatureGateMiddleware(next, "orders", new FeatureGateOptions());

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_FlagDisabledWithCustomStatusCode_ReturnsConfiguredStatusCode()
    {
        var context = new DefaultHttpContext();
        var featureFlagService = new Mock<IFeatureFlagService>();
        featureFlagService
            .Setup(s => s.IsEnabledAsync("orders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var services = new ServiceCollection()
            .AddSingleton(featureFlagService.Object)
            .BuildServiceProvider();
        context.RequestServices = services;

        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = new FeatureGateOptions { DisabledStatusCode = StatusCodes.Status403Forbidden };
        var middleware = new FeatureGateMiddleware(next, "orders", options);

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_FlagDisabledWithRedirectPath_DoesNotCallNextAndRedirects()
    {
        var context = new DefaultHttpContext();
        var featureFlagService = new Mock<IFeatureFlagService>();
        featureFlagService
            .Setup(s => s.IsEnabledAsync("orders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var services = new ServiceCollection()
            .AddSingleton(featureFlagService.Object)
            .BuildServiceProvider();
        context.RequestServices = services;

        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = new FeatureGateOptions { DisabledRedirectPath = "/feature-disabled" };
        var middleware = new FeatureGateMiddleware(next, "orders", options);

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.Equal("/feature-disabled", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenFeatureFlagServiceIsMissing_ThrowsInvalidOperationException()
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new FeatureGateMiddleware(next, "orders", new FeatureGateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
    }
}