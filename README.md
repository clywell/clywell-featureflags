# Clywell.Core.FeatureFlags

Feature flag evaluation engine for .NET — rule-based rollout, tenant and user targeting, percentage rollouts, and pluggable providers. Zero infrastructure dependency for Application layer usage.

<!-- Badges -->
[![Clywell.Core.FeatureFlags](https://img.shields.io/nuget/v/Clywell.Core.FeatureFlags.svg?label=Clywell.Core.FeatureFlags)](https://www.nuget.org/packages/Clywell.Core.FeatureFlags/)
[![Clywell.Core.FeatureFlags.AspNetCore](https://img.shields.io/nuget/v/Clywell.Core.FeatureFlags.AspNetCore.svg?label=Clywell.Core.FeatureFlags.AspNetCore)](https://www.nuget.org/packages/Clywell.Core.FeatureFlags.AspNetCore/)
[![License](https://img.shields.io/github/license/clywell/clywell-featureflags.svg)](LICENSE)

## Packages

| Package | Description |
|---------|-------------|
| `Clywell.Core.FeatureFlags` | Core evaluation engine. No ASP.NET Core dependency — safe for Application layer, workers, and background services. |
| `Clywell.Core.FeatureFlags.AspNetCore` | HTTP enforcement primitives: MVC action filter attribute, request-level middleware gate, and Minimal API endpoint filter. |

---

## Overview

`Clywell.Core.FeatureFlags` is a lean, boolean-only feature flag engine with a fully pluggable provider model. It evaluates flags against rules and targeting conditions without bundled storage, caching, or UI. You bring the data source; the engine handles the evaluation.

**Design principles:**
- **Boolean only** — flags are on or off; no strings, variants, or JSON payloads
- **Provider-owned caching** — the engine resolves flags from your `IFeatureFlagProvider` on every evaluation; cache at the provider layer if needed
- **Zero infrastructure** — `Clywell.Core.FeatureFlags` depends only on `Microsoft.Extensions.DependencyInjection.Abstractions`; use it in any .NET project
- **Composable conditions** — combine built-in conditions with logical operators using a fluent API
- **Priority-ordered rules** — the highest-priority matching rule wins; fall back to `DefaultValue` when no rule matches

---

## Installation

Core package (required):

```bash
dotnet add package Clywell.Core.FeatureFlags
```

ASP.NET Core enforcement (optional):

```bash
dotnet add package Clywell.Core.FeatureFlags.AspNetCore
```

---

## Table of Contents

- [Quick Start](#quick-start)
- [Implementing a Provider](#implementing-a-provider)
- [Evaluation Context](#evaluation-context)
- [Evaluating Flags](#evaluating-flags)
- [Building Flags with FeatureFlagBuilder](#building-flags-with-featureflagbuilder)
- [Conditions Reference](#conditions-reference)
- [Composing Conditions](#composing-conditions)
- [ASP.NET Core Integration](#aspnet-core-integration)
  - [MVC Action Filter](#mvc-action-filter)
  - [Minimal API Endpoint Filter](#minimal-api-endpoint-filter)
  - [Middleware Gate](#middleware-gate)
- [Options Reference](#options-reference)
- [API Reference](#api-reference)

---

## Quick Start

### 1. Register services

```csharp
// Core only (workers, background services, Application layer)
builder.Services.AddFeatureFlags();

// ASP.NET Core apps — registers both Core and ASP.NET Core primitives
builder.Services.AddFeatureFlagsAspNetCore();

// Always register your own provider
builder.Services.AddScoped<IFeatureFlagProvider, MyFeatureFlagProvider>();
```

### 2. Evaluate in application code

```csharp
public class CheckoutHandler(IFeatureFlagService flags)
{
    public async Task Handle(CheckoutCommand cmd, CancellationToken ct)
    {
        // Empty context
        if (!await flags.IsEnabledAsync("new-checkout", ct))
            return;

        // Tenant + user context via builder delegate
        if (!await flags.IsEnabledAsync("new-checkout", ctx => ctx
                .WithTenant(cmd.TenantId)
                .WithUser(cmd.UserId), ct))
            return;

        // Evaluate multiple flags at once
        var results = await flags.EvaluateManyAsync(
            ["new-checkout", "express-shipping"],
            ctx => ctx.WithTenant(cmd.TenantId));
    }
}
```

### 3. Gate an MVC endpoint

```csharp
[RequiresFeature("new-checkout")]
public IActionResult Checkout() => Ok();
```

### 4. Gate a Minimal API route

```csharp
app.MapPost("/checkout", handler).RequireFeature("new-checkout");
```

---

## Implementing a Provider

Implement `IFeatureFlagProvider` to connect the engine to your data source (database, config server, JSON file, etc.).

```csharp
public class MyFeatureFlagProvider : IFeatureFlagProvider
{
    private readonly IFlagRepository _repo;

    public MyFeatureFlagProvider(IFlagRepository repo) => _repo = repo;

    public async Task<IEnumerable<FeatureFlag>> GetAllAsync(CancellationToken ct = default)
        => await _repo.GetAllFlagsAsync(ct);

    public async Task<FeatureFlag?> GetAsync(string key, CancellationToken ct = default)
        => await _repo.FindByKeyAsync(key, ct);
}
```

**Caching is provider-owned.** The engine calls your provider on every evaluation. Wrap a memory cache inside your provider if you want to avoid repeated database calls:

```csharp
public async Task<FeatureFlag?> GetAsync(string key, CancellationToken ct = default)
    => await _cache.GetOrCreateAsync(key, entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return _repo.FindByKeyAsync(key, ct);
    });
```

---

## Evaluation Context

`EvaluationContext` carries the targeting data used by conditions during evaluation.

```csharp
// Empty context — use when no targeting is required
var ctx = EvaluationContext.Empty;

// Build a context with the fluent builder
var ctx = new EvaluationContextBuilder()
    .WithTenant("tenant-abc")
    .WithUser("user-xyz")
    .WithAttribute("plan", "enterprise")
    .WithAttribute("region", "eu-west")
    .Build();
```

`EvaluationContext` is an immutable `sealed record`. `Attributes` is backed by a `FrozenDictionary` — zero-allocation lookups at evaluation time.

---

## Evaluating Flags

`IFeatureFlagService` provides three overloads:

```csharp
// 1. No context
bool on = await flags.IsEnabledAsync("flag-key", ct);

// 2. Pre-built context
var ctx = new EvaluationContextBuilder().WithTenant(tenantId).Build();
bool on = await flags.IsEnabledAsync("flag-key", ctx, ct);

// 3. Builder delegate (most common)
bool on = await flags.IsEnabledAsync("flag-key", ctx => ctx.WithTenant(tenantId), ct);
```

Convenience extensions for common cases:

```csharp
// Tenant only
bool on = await flags.IsEnabledAsync("flag-key", tenantId, ct);

// Tenant + user
bool on = await flags.IsEnabledAsync("flag-key", tenantId, userId, ct);

// Evaluate multiple flags in one call
IReadOnlyDictionary<string, bool> results = await flags.EvaluateManyAsync(
    ["flag-a", "flag-b", "flag-c"],
    ctx => ctx.WithTenant(tenantId), ct);
```

---

## Building Flags with FeatureFlagBuilder

Use `FeatureFlagBuilder` inside your `IFeatureFlagProvider` implementation to construct `FeatureFlag` instances with a fluent API.

```csharp
FeatureFlag flag = FeatureFlagBuilder
    .For("new-checkout")
    .WithDescription("Gradual rollout of the redesigned checkout flow")
    .DisabledByDefault()
    .EnableWhen(new TenantCondition("tenant-early-access"))
    .EnableWhen(new AttributeCondition("plan", "enterprise"))
    .EnableWhen(new PercentageCondition("new-checkout", 10))
    .Build();
```

Rules added first receive the highest auto-assigned priority. Pass an explicit priority value to override:

```csharp
.EnableWhen(new TenantCondition("tenant-vip"), priority: 100)
.EnableWhen(new PercentageCondition("new-checkout", 20), priority: 50)
```

---

## Conditions Reference

All built-in conditions implement `IEvaluationCondition`.

| Condition | Match behaviour |
|-----------|----------------|
| `AlwaysCondition` | Always `true`. Use as a lowest-priority catch-all to enable a feature for everyone. |
| `TenantCondition(params string[])` | `true` when `context.TenantId` is in the supplied set (case-insensitive). |
| `UserCondition(params string[])` | `true` when `context.UserId` is in the supplied set (case-insensitive). |
| `AttributeCondition(key, value, comparison?)` | `true` when `context.Attributes[key]` equals `value` using the specified `StringComparison` (default: `OrdinalIgnoreCase`). |
| `PercentageCondition(flagKey, percentage)` | `true` for a deterministic `percentage`% of the audience derived from a FNV-1a hash of `flagKey + userId/tenantId`. Same user/tenant always falls in the same bucket for the same flag. |
| `AllOfCondition(conditions)` | Logical AND — `true` only when every inner condition matches. |
| `AnyOfCondition(conditions)` | Logical OR — `true` when at least one inner condition matches. |
| `NotCondition(inner)` | Logical NOT — inverts the result of the inner condition. |

```csharp
// TenantCondition — single or multiple tenants
new TenantCondition("tenant-a")
new TenantCondition("tenant-a", "tenant-b", "tenant-c")

// UserCondition — single or multiple users
new UserCondition("user-xyz")
new UserCondition("user-abc", "user-def")

// AttributeCondition — custom attribute matching
new AttributeCondition("plan", "enterprise")
new AttributeCondition("region", "eu-west", StringComparison.Ordinal)

// PercentageCondition — 10% deterministic rollout
new PercentageCondition("new-checkout", 10)

// AlwaysCondition — catch-all
AlwaysCondition.Instance
```

---

## Composing Conditions

Use the `Condition` static factory or fluent extension methods to build complex targeting expressions.

### Static factory

```csharp
// All conditions must match (AND)
Condition.AllOf(
    new TenantCondition("tenant-a"),
    new AttributeCondition("plan", "enterprise"))

// At least one must match (OR)
Condition.AnyOf(
    new TenantCondition("tenant-a"),
    new UserCondition("user-xyz"))

// Invert a condition (NOT)
Condition.Not(new TenantCondition("tenant-blocked"))
```

### Fluent extensions

```csharp
// .And() — both must match
new TenantCondition("tenant-a")
    .And(new AttributeCondition("plan", "enterprise"))

// .Or() — either must match
new TenantCondition("tenant-a")
    .Or(new UserCondition("user-xyz"))

// .Negate() — invert
new TenantCondition("tenant-blocked").Negate()
```

### Complex example

```csharp
FeatureFlagBuilder
    .For("new-checkout")
    .DisabledByDefault()
    // Enable for enterprise tenants in the EU
    .EnableWhen(
        new TenantCondition("tenant-enterprise")
            .And(new AttributeCondition("region", "eu")))
    // OR: enable for beta users
    .EnableWhen(new UserCondition("user-beta-1", "user-beta-2"))
    // Block a specific tenant explicitly
    .DisableWhen(new TenantCondition("tenant-blocked"), priority: 1000)
    .Build();
```

---

## ASP.NET Core Integration

Register ASP.NET Core primitives alongside the core engine:

```csharp
builder.Services.AddFeatureFlagsAspNetCore(options =>
{
    options.DisabledStatusCode = StatusCodes.Status503ServiceUnavailable;
    // or redirect instead:
    // options.DisabledRedirectPath = "/maintenance";
});

// Always register your provider too
builder.Services.AddScoped<IFeatureFlagProvider, MyFeatureFlagProvider>();
```

`AddFeatureFlagsAspNetCore` automatically calls `AddFeatureFlags` — you do not need both calls.

> **Context note:** All three HTTP enforcement primitives evaluate with `EvaluationContext.Empty`. They gate at the HTTP boundary before your application code resolves user/tenant identity. If you need tenant- or user-aware gating, use `IFeatureFlagService` directly inside your handler or middleware.

---

### MVC Action Filter

`[RequiresFeature]` is an `ActionFilterAttribute` for MVC controllers and Razor Pages.

```csharp
[RequiresFeature("new-checkout")]
public IActionResult Checkout() => Ok();

// Controller-level — gates all actions
[RequiresFeature("beta-dashboard")]
public class BetaController : Controller { ... }
```

> **Important:** `[RequiresFeature]` only works on MVC controllers and Razor Pages. It is silently ignored on Minimal API endpoints. Use `.RequireFeature()` for Minimal APIs.

---

### Minimal API Endpoint Filter

`.RequireFeature()` adds an `IEndpointFilter` to any `IEndpointConventionBuilder` — works with route groups too.

```csharp
// Single endpoint
app.MapPost("/checkout", handler)
   .RequireFeature("new-checkout");

// Route group — gates all routes in the group
var beta = app.MapGroup("/beta")
              .RequireFeature("beta-features");

beta.MapGet("/dashboard", dashboardHandler);
beta.MapGet("/reports", reportsHandler);
```

---

### Middleware Gate

`UseFeatureGate` blocks an entire path prefix when a flag is disabled.

```csharp
// Return 503 when the flag is off
app.UseFeatureGate("maintenance-mode");

// Redirect to a specific path when the flag is off
app.UseFeatureGate("new-api", disabledPath: "/api/v1");
```

---

## Options Reference

### `FeatureFlagOptions` (core)

| Property | Default | Description |
|----------|---------|-------------|
| `DefaultValueWhenNotFound` | `false` | Return value when the requested flag key is not found in the provider. |

```csharp
builder.Services.AddFeatureFlags(options =>
    options.WithDefaultValueWhenNotFound(false));
```

### `FeatureGateOptions` (ASP.NET Core)

| Property | Default | Description |
|----------|---------|-------------|
| `DisabledStatusCode` | `404` | HTTP status code returned when a gate blocks a request and no redirect is configured. |
| `DisabledRedirectPath` | `null` | When set, gates redirect to this path instead of returning `DisabledStatusCode`. Must be a relative or absolute URL. |

```csharp
builder.Services.AddFeatureFlagsAspNetCore(options =>
{
    options.DisabledStatusCode = StatusCodes.Status503ServiceUnavailable;
    options.DisabledRedirectPath = "/maintenance";
});
```

---

## API Reference

### `IFeatureFlagService`

| Method | Description |
|--------|-------------|
| `IsEnabledAsync(key, ct)` | Evaluate with empty context. |
| `IsEnabledAsync(key, context, ct)` | Evaluate with a pre-built `EvaluationContext`. |
| `IsEnabledAsync(key, configure, ct)` | Evaluate with a builder delegate. |

### `FeatureFlagServiceExtensions`

| Method | Description |
|--------|-------------|
| `IsEnabledAsync(key, tenantId, ct)` | Shortcut — evaluates with tenant context only. |
| `IsEnabledAsync(key, tenantId, userId, ct)` | Shortcut — evaluates with tenant + user context. |
| `EvaluateManyAsync(keys, context, ct)` | Evaluate multiple flags, returns `IReadOnlyDictionary<string, bool>`. |
| `EvaluateManyAsync(keys, configure, ct)` | Same, with builder delegate. |

### `IFeatureFlagProvider`

| Method | Description |
|--------|-------------|
| `GetAllAsync(ct)` | Returns all flags. Used by evaluators that need the full set. |
| `GetAsync(key, ct)` | Returns a single flag by key, or `null` if not found. |

### `FeatureFlagBuilder`

| Method | Description |
|--------|-------------|
| `For(key)` | Static factory — start building a flag with the given key. |
| `WithDescription(desc)` | Set an optional human-readable description. |
| `EnabledByDefault()` | Set `DefaultValue = true`. |
| `DisabledByDefault()` | Set `DefaultValue = false` (default). |
| `EnableWhen(condition, priority?)` | Add a rule that enables the flag when the condition matches. |
| `DisableWhen(condition, priority?)` | Add a rule that disables the flag when the condition matches. |
| `Build()` | Produce the immutable `FeatureFlag` instance. |
