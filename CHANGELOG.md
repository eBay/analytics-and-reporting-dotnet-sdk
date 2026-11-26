# ChangeLog

## 0.5.0 (11/26/2026)

### Added

- Local mock servers: base URL properties accept plain `http` when the host is a loopback host (`127.0.0.1`, `::1` or `localhost`) that you also add to `AnalyticsAndReportingConfiguration.AllowedHosts`. Before, every base URL had to use HTTPS. Every other host still requires HTTPS. `AnalyticsAndReportingConfiguration.LoopbackHosts` lists the loopback hosts.

### Changed

- Hosts are compared case-insensitively and without IPv6 brackets, so `[::1]` and `::1` match.

## 0.4.0 (11/17/2026)

### Breaking changes

#### Simpler dependency injection

- `AddAnalyticsAndReportingSdk` now registers only the configuration and a singleton `AnalyticsAndReportingClient`. `AnalyticsClient` is no longer registered individually: inject `AnalyticsAndReportingClient` and use its `Analytics` property.
- The SDK no longer uses `IHttpClientFactory` or registers named `HttpClient`s, and no longer depends on `Microsoft.Extensions.Http`. To customise HTTP (proxy, client certificates, test doubles), use the new `AnalyticsAndReportingClient(AnalyticsAndReportingConfiguration, HttpMessageHandler)` constructor.

#### Trusted hosts

- The client constructors throw `InvalidOperationException` when a base URL uses a host that is not trusted (see Security). Add such hosts, for example a proxy, to `AnalyticsAndReportingConfiguration.AllowedHosts`.

### Security

- The access token is sent only to trusted hosts. Before, it was attached to requests for any HTTPS host configured as a base URL.

### Added

- `AnalyticsAndReportingConfiguration.AllowedHosts` for additional hosts trusted with the access token, and `AnalyticsAndReportingConfiguration.DefaultAllowedHosts`. The eBay production hosts and their sandbox counterparts (for example `api.sandbox.ebay.com`) are trusted by default, so the sandbox needs no extra configuration.
- Target framework `net472` (.NET Framework 4.7.2).

### Changed

- Dependency minimums are lowered to the versions Kiota itself requires, so projects can keep older compatible versions: `Microsoft.Kiota.Bundle` >= 1.22.0 and `Microsoft.Extensions.DependencyInjection.Abstractions` >= 6.0.0. Previously the SDK required the newest 10.0.x packages. On .NET Framework the SDK requires `System.Text.Json` >= 10.0.0, matching the oldest supported .NET LTS (.NET 10). None of these minimums has a known vulnerability, and each release is tested at both the minimum and the latest versions.
- On .NET Framework the SDK's HttpClients allow 64 connections per host instead of the `ServicePointManager` default of 2.
- `AnalyticsAndReportingConfiguration.HttpTimeout` and `RequestOptions.Headers` have `set` accessors instead of `init`, so projects compiled with C# 7.3 can assign them. Existing source compiles unchanged; recompile code that was built against 0.3.0.

## 0.3.0 (09/25/2026)

### Added

- Target frameworks `net8.0` and `net9.0`, in addition to `net10.0`.

## 0.2.0 (09/14/2026)

### Breaking changes

#### Renamed clients and members

| Previous | Current |
| --- | --- |
| `AnalyticsRestSubClient` | `AnalyticsClient` |

#### Changed signatures

`AnalyticsClient`:

| Operation | Previous parameters | Current parameters |
| --- | --- | --- |
| `GetTrafficReportAsync` | `(string? dimension, string? filter, string? metric, string? sort, string? accessToken, RequestOptions? requestOptions, CancellationToken cancellationToken)` | `(string dimension, string filter, string metric, string? sort, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
