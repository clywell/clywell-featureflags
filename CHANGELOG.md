# Changelog

All notable changes to Clywell.Core.FeatureFlags will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-03-05

### Added

#### `Clywell.Core.FeatureFlags`

**Core engine**
- `IFeatureFlagService` — primary evaluation interface with three `IsEnabledAsync` overloads: empty context, pre-built `EvaluationContext`, and builder delegate
- `IFeatureFlagProvider` — single interface consumers must implement to connect any data source; exposes `GetAllAsync` and `GetAsync(key)`
- `IFeatureFlagEvaluator` — optional override point for custom evaluation logic; the default implementation ships in-box
- `DefaultFeatureFlagEvaluator` — evaluates rules in descending priority order; returns the first matching rule's value, or `flag.DefaultValue` when no rule matches
- `FeatureFlag` — immutable `sealed record`; carries `Key`, `Description`, `DefaultValue`, and an ordered list of `EvaluationRule` instances
- `EvaluationRule` — immutable `sealed record`; carries `Priority`, `Condition`, and `Value` (enabled/disabled)
- `EvaluationContext` — immutable `sealed record`; carries `TenantId`, `UserId`, and a `FrozenDictionary<string, string>` of custom attributes; static `Empty` singleton for no-context evaluations
- `EvaluationContextBuilder` — fluent builder: `WithTenant()`, `WithUser()`, `WithAttribute()`, `Build()`
- `FeatureFlagOptions` — `DefaultValueWhenNotFound` (default `false`) configures the engine's fallback when a flag key is not found in the provider
- `ServiceCollectionExtensions.AddFeatureFlags(Action<FeatureFlagOptions>?)` — registers `IFeatureFlagEvaluator` (singleton), `IFeatureFlagService` (scoped), and `FeatureFlagOptions` (singleton) using `TryAdd*`; `IFeatureFlagProvider` is intentionally not auto-registered — consumers must register their own

**Fluent flag builder**
- `FeatureFlagBuilder` — fluent provider-side builder: `For(key)`, `WithDescription()`, `EnabledByDefault()`, `DisabledByDefault()`, `EnableWhen(condition, priority?)`, `DisableWhen(condition, priority?)`, `Build()`; rules added first receive the highest auto-assigned priority

**Built-in conditions**
- `AlwaysCondition` — always returns `true`; `AlwaysCondition.Instance` singleton for catch-all rules
- `TenantCondition(params string[])` — matches when `context.TenantId` is in the supplied set (case-insensitive, backed by `FrozenSet`)
- `UserCondition(params string[])` — matches when `context.UserId` is in the supplied set (case-insensitive, backed by `FrozenSet`)
- `AttributeCondition(key, value, comparison?)` — matches when `context.Attributes[key]` equals the expected value using the configured `StringComparison` (default: `OrdinalIgnoreCase`)
- `PercentageCondition(flagKey, percentage)` — deterministic percentage rollout using FNV-1a 32-bit hashing; the same user/tenant always falls in the same bucket for the same flag key; requires `context.UserId` or `context.TenantId`
- `AllOfCondition(conditions)` — logical AND composite; vacuously `true` for an empty set
- `AnyOfCondition(conditions)` — logical OR composite; vacuously `false` for an empty set
- `NotCondition(inner)` — logical NOT; inverts the result of the inner condition

**Condition composition**
- `Condition` — static factory: `AllOf(params)`, `AnyOf(params)`, `Not(inner)`
- Extension methods on `IEvaluationCondition`: `.And(right)`, `.Or(right)`, `.Negate()`

**Service extensions**
- `FeatureFlagServiceExtensions` — convenience extensions on `IFeatureFlagService`:
  - `IsEnabledAsync(key, tenantId, ct)` — shortcut for tenant-only context
  - `IsEnabledAsync(key, tenantId, userId, ct)` — shortcut for tenant + user context
  - `EvaluateManyAsync(keys, context, ct)` — evaluate multiple flags, returns `IReadOnlyDictionary<string, bool>`
  - `EvaluateManyAsync(keys, configure, ct)` — same with builder delegate

#### `Clywell.Core.FeatureFlags.AspNetCore`

- `RequiresFeatureAttribute(flagKey)` — MVC `ActionFilterAttribute`; short-circuits execution and returns the configured `DisabledStatusCode` (or redirects to `DisabledRedirectPath`) when the flag is disabled; evaluates with `EvaluationContext.Empty`; **does not work on Minimal API endpoints** — use `.RequireFeature()` instead
- `FeatureGateMiddleware` — request-level middleware that blocks an entire path when a feature flag is disabled; evaluates with `EvaluationContext.Empty`
- `ApplicationBuilderExtensions.UseFeatureGate(key, disabledPath?)` — registers `FeatureGateMiddleware` for the given flag key
- `FeatureFlagEndpointFilter` — `IEndpointFilter` for Minimal APIs; short-circuits the endpoint pipeline when the flag is disabled; evaluates with `EvaluationContext.Empty`
- `FeatureFlagEndpointRouteBuilderExtensions.RequireFeature<TBuilder>(flagKey)` — extension on `IEndpointConventionBuilder`; adds `FeatureFlagEndpointFilter` to any Minimal API endpoint or route group
- `FeatureGateOptions` — `DisabledStatusCode` (default `404`) and `DisabledRedirectPath?` (default `null`)
- `ServiceCollectionExtensions.AddFeatureFlagsAspNetCore(Action<FeatureGateOptions>?)` — registers ASP.NET Core gate services and automatically calls `AddFeatureFlags`; consumers only need one call
