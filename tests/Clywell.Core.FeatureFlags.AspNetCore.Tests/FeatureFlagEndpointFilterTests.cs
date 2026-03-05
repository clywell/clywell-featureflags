namespace Clywell.Core.FeatureFlags.AspNetCore.Tests;

public sealed class FeatureFlagEndpointFilterTests
{
    [Fact]
    public async Task InvokeAsync_FlagEnabled_InvokesNextAndReturnsNextValue()
    {
        var context = CreateInvocationContext(isEnabled: true, options: new FeatureGateOptions());
        var filter = new FeatureFlagEndpointFilter("orders");

        var nextCalled = false;
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("ok");
        };

        var result = await filter.InvokeAsync(context, next);

        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task InvokeAsync_FlagDisabledWithoutRedirect_Returns404AndDoesNotInvokeNext()
    {
        var context = CreateInvocationContext(isEnabled: false, options: new FeatureGateOptions());
        var filter = new FeatureFlagEndpointFilter("orders");

        var nextCalled = false;
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("ok");
        };

        var result = await filter.InvokeAsync(context, next);

        Assert.False(nextCalled);
        await AssertStatusCodeAsync(result, StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task InvokeAsync_FlagDisabledWithCustomStatusCode_ReturnsConfiguredStatusCode()
    {
        var options = new FeatureGateOptions { DisabledStatusCode = StatusCodes.Status403Forbidden };
        var context = CreateInvocationContext(isEnabled: false, options: options);
        var filter = new FeatureFlagEndpointFilter("orders");

        var nextCalled = false;
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("ok");
        };

        var result = await filter.InvokeAsync(context, next);

        Assert.False(nextCalled);
        await AssertStatusCodeAsync(result, StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_FlagDisabledWithRedirect_ReturnsRedirectAndDoesNotInvokeNext()
    {
        var options = new FeatureGateOptions { DisabledRedirectPath = "/feature-disabled" };
        var context = CreateInvocationContext(isEnabled: false, options: options);
        var filter = new FeatureFlagEndpointFilter("orders");

        var nextCalled = false;
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("ok");
        };

        var result = await filter.InvokeAsync(context, next);

        Assert.False(nextCalled);
        await AssertRedirectAsync(result, "/feature-disabled");
    }

    private static EndpointFilterInvocationContext CreateInvocationContext(bool isEnabled, FeatureGateOptions options)
    {
        var featureFlagService = new Mock<IFeatureFlagService>();
        featureFlagService
            .Setup(s => s.IsEnabledAsync("orders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(isEnabled);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(featureFlagService.Object)
            .AddSingleton(options)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        return new TestEndpointFilterInvocationContext(httpContext);
    }

    private static async Task AssertStatusCodeAsync(object? result, int expectedStatusCode)
    {
        var iResult = Assert.IsAssignableFrom<IResult>(result);
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
        };

        await iResult.ExecuteAsync(httpContext);

        Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);
    }

    private static async Task AssertRedirectAsync(object? result, string expectedPath)
    {
        var iResult = Assert.IsAssignableFrom<IResult>(result);
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
        };

        await iResult.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status302Found, httpContext.Response.StatusCode);
        Assert.Equal(expectedPath, httpContext.Response.Headers.Location.ToString());
    }

    private sealed class TestEndpointFilterInvocationContext(HttpContext httpContext) : EndpointFilterInvocationContext
    {
        private readonly List<object?> _arguments = [];

        public override HttpContext HttpContext { get; } = httpContext;

        public override IList<object?> Arguments => _arguments;

        public override T GetArgument<T>(int index)
        {
            return (T)Arguments[index]!;
        }
    }
}