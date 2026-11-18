# eBay Analytics and Reporting SDK for .NET

Read seller standards profiles, customer service metrics and listing traffic reports. This SDK wraps the eBay Analytics API behind one client, `AnalyticsAndReportingClient`.

- **Target frameworks:** .NET Framework 4.7.2, .NET 8, .NET 9 and .NET 10
- **C#:** usable from C# 7.3 and later

## Installation

The package `EBay.AnalyticsAndReporting` is not on nuget.org yet. Reference the project, or build the package and add it to a local feed:

```bash
dotnet pack EBay.AnalyticsAndReporting -c Release -o artifacts/packages
```

## Quick start

```csharp
using EBay.AnalyticsAndReporting;
using EBay.AnalyticsAndReporting.Configuration;

var config = new AnalyticsAndReportingConfiguration
{
    // A user or application access token from your own OAuth flow (see "Authentication").
    AccessToken = Environment.GetEnvironmentVariable("EBAY_ACCESS_TOKEN")
};

using var client = new AnalyticsAndReportingClient(config);

var profiles = await client.Analytics.FindSellerStandardsProfilesAsync();
Console.WriteLine($"{profiles?.StandardsProfiles?.Count} standards profiles");
```

## APIs

| `AnalyticsAndReportingClient` property | API | Default base URL | Access token |
|---|---|---|---|
| `Analytics` | Analytics API | `https://api.ebay.com/sell/analytics/v1` | User |

"Access token" is what eBay's OpenAPI spec declares: an application token (client credentials) or a user token (authorization code). Scopes are listed per API below.

<details>
<summary><b>Analytics</b>: Analytics API, 4 operations</summary>

Scopes: `https://api.ebay.com/oauth/api_scope/sell.analytics.readonly`

| Method | Description |
|---|---|
| `GetCustomerServiceMetricAsync` | Retrieves the seller's customer service performance for the specified metric type and eBay marketplace. |
| `FindSellerStandardsProfilesAsync` | This call retrieves all the standards profiles for the associated seller. |
| `GetSellerStandardsProfileAsync` | This call retrieves a single standards profile for the associated seller. |
| `GetTrafficReportAsync` | This method returns a report that details the user traffic received by a seller's listings. |

</details>

## Authentication

The SDK does not obtain or refresh tokens: get them with your own OAuth flow ([eBay OAuth guide](https://developer.ebay.com/develop/guides/sell/authorization)) and give them to the SDK in one of two ways:

- `AnalyticsAndReportingConfiguration.AccessToken`: used for every call that doesn't pass its own token.
- The `accessToken` parameter on every method: used for that call only, for example a user token where the configuration holds an application token.

```csharp
string userToken = Environment.GetEnvironmentVariable("EBAY_USER_TOKEN")!;
var profiles = await client.Analytics.FindSellerStandardsProfilesAsync(accessToken: userToken);
```

A token is only ever sent to trusted hosts, see below.

## Configuration

| Property | Type | Default | Description |
|---|---|---|---|
| `AnalyticsBaseUrl` | `string` | `https://api.ebay.com/sell/analytics/v1` | Base URL of the Analytics API. |
| `AccessToken` | `string?` |  | Fallback bearer token used when no per-call accessToken is supplied. |
| `DiagnosticLogging` | `bool` | `false` | When true, logs the HTTP method, URL, status code, and response reason phrase for all non-success responses to stderr. |
| `HttpTimeout` | `TimeSpan` | 30 seconds | Timeout applied to all HTTP requests. |
| `DefaultRequestOptions` | `RequestOptions` | no headers | Default headers applied to every REST request. |
| `AllowedHosts` | `IList<string>` | empty | Additional hosts trusted with the access token, for example a proxy (see below). |

`AnalyticsAndReportingConfiguration.DefaultAllowedHosts` lists the hosts trusted by default.

## Sandbox and trusted hosts

The access token is only sent to trusted hosts: the hosts in `AnalyticsAndReportingConfiguration.DefaultAllowedHosts`, which are the eBay production hosts and their `*.sandbox.ebay.com` counterparts, plus any in `AllowedHosts`. Switching to the sandbox needs only the base URLs:

```csharp
var sandboxConfig = new AnalyticsAndReportingConfiguration
{
    AccessToken = Environment.GetEnvironmentVariable("EBAY_SANDBOX_ACCESS_TOKEN"),
    AnalyticsBaseUrl = "https://api.sandbox.ebay.com/sell/analytics/v1",
};
```

Any other host, for example a proxy that forwards to eBay, has to be added to `AllowedHosts`; the client constructor throws `InvalidOperationException` otherwise:

```csharp
var proxied = new AnalyticsAndReportingConfiguration
{
    AnalyticsBaseUrl = "https://ebay-proxy.example.com/sell/analytics/v1",
    AllowedHosts = { "ebay-proxy.example.com" },
};
```

## Dependency injection

`AddAnalyticsAndReportingSdk` registers the configuration and one shared `AnalyticsAndReportingClient` as a singleton. It owns its HTTP connections and is disposed with the container. Inject the client and use its properties for the individual APIs:

```csharp
using EBay.AnalyticsAndReporting;
using EBay.AnalyticsAndReporting.Configuration;
using EBay.AnalyticsAndReporting.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAnalyticsAndReportingSdk(new AnalyticsAndReportingConfiguration
{
    AccessToken = builder.Configuration["EBay:AccessToken"]
});

var app = builder.Build();
app.MapGet("/example", async (AnalyticsAndReportingClient client) =>
{
    var profiles = await client.Analytics.FindSellerStandardsProfilesAsync();
    return Results.Ok();
});
app.Run();
```

## Custom HTTP handler

To send requests through your own handler, for example for a proxy, client certificates or a test double, pass it to the constructor. The handler is shared by all APIs and is not disposed by the client:

```csharp
var handler = new HttpClientHandler { Proxy = new WebProxy("http://corporate-proxy:8080") };
using var proxiedClient = new AnalyticsAndReportingClient(config, handler);
```

## Error handling

```csharp
try
{
    var profiles = await client.Analytics.FindSellerStandardsProfilesAsync();
}
catch (UnauthorizedException)
{
    // HTTP 401: the token is missing, expired, for the other environment, or lacks a scope.
}
catch (ApiException ex)
{
    // Any other REST error.
    Console.WriteLine($"HTTP {ex.ResponseStatusCode}");
}
```

- `UnauthorizedException` (HTTP 401) derives from `SdkException`.
- Other REST errors throw Kiota's `ApiException`, or a generated subclass named in eBay's spec for that operation, with `ResponseStatusCode`. eBay's error text is only available where the spec describes the error body correctly; set `DiagnosticLogging = true` to log failed requests to stderr.

## .NET Framework

The `net472` build is a classic package that `packages.config` projects can use. Such projects must list every dependency themselves. At the minimum versions the SDK supports, these are:

| Package | Version |
|---|---|
| Microsoft.Kiota.Abstractions, .Bundle, .Http.HttpClientLibrary, .Serialization.Form, .Serialization.Json, .Serialization.Multipart, .Serialization.Text | 1.22.0 |
| Std.UriTemplate | 2.0.8 |
| Microsoft.IO.RecyclableMemoryStream | 3.0.1 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 6.0.0 |
| System.Diagnostics.DiagnosticSource, System.Net.Http.WinHttpHandler | 6.0.0 |
| System.Text.Json, System.Text.Encodings.Web, System.IO.Pipelines, Microsoft.Bcl.AsyncInterfaces | 10.0.0 |
| System.Runtime.CompilerServices.Unsafe | 6.1.2 |
| System.Memory, System.Threading.Tasks.Extensions | 4.6.3 |
| System.Buffers, System.Numerics.Vectors, System.ValueTuple | 4.6.1 |

With these versions the app needs binding redirects for Microsoft.Bcl.AsyncInterfaces (10.0.0.0), System.Buffers (4.0.5.0), System.Memory (4.0.5.0), System.Runtime.CompilerServices.Unsafe (6.0.3.0), System.Text.Encodings.Web (10.0.0.0), System.Text.Json (10.0.0.0) and System.Threading.Tasks.Extensions (4.2.4.0). There must be no redirect for System.ValueTuple: .NET Framework 4.7.1+ provides it. Newer package versions work too and change the redirect versions.

## Development

- .NET 10 SDK, plus the .NET 8 and .NET 9 SDKs so every target framework builds. `net472` builds on any OS; its tests run on Windows only.
- Build: `dotnet build EBay.AnalyticsAndReporting`
- Test: `dotnet test tests/EBay.AnalyticsAndReporting.Tests`
- Most of this repository is generated from eBay's API specs by the SDK generator (`api-sdk-definitions`): the API clients, `Configuration/`, `Extensions/`, `AnalyticsAndReportingClient.cs`, `specs/` and `generate.sh`. Change the generator, not the generated files. `./generate.sh` reruns Kiota for the specs in `specs/openapi`.

See [CONTRIBUTING.adoc](CONTRIBUTING.adoc).

## License

Apache 2.0, see [LICENSE](LICENSE).
