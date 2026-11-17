# Analytics and Reporting .NET SDK

A .NET SDK for eBay Analytics and Reporting APIs.

## Features

- **Target frameworks**: .NET Framework 4.7.2, .NET 8, .NET 9 and .NET 10
- **Analytics API**: Complete coverage of seller performance metrics and reporting (4 operations)
- **OAuth Authentication**: Token refresh handler interface for seamless authentication
- **Dependency Injection**: First-class support for ASP.NET Core DI
- **Strong Typing**: Internal generated types with public abstraction layer

## Architecture

```
EBay.AnalyticsAndReporting/
├── Generated/Analytics/          # Kiota-generated REST client (internal)
├── Configuration/                # SDK configuration and auth
├── Exceptions/                   # SDK exceptions
├── Extensions/                   # DI extension methods
└── AnalyticsAndReportingClient.cs  # Main SDK entry point
```

## Installation

```bash
# From source
dotnet pack EBay.AnalyticsAndReporting/EBay.AnalyticsAndReporting.csproj
dotnet add reference path/to/EBay.AnalyticsAndReporting.1.0.0.nupkg

# Or add project reference
dotnet add reference EBay.AnalyticsAndReporting/EBay.AnalyticsAndReporting.csproj
```

## Quick Start

### 1. Implement Token Refresh Handler

```csharp
using EBay.AnalyticsAndReporting.Configuration;

public class MyTokenRefreshHandler : ITokenRefreshHandler
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public MyTokenRefreshHandler(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            return _accessToken;

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.ebay.com/identity/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}")));

        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "https://api.ebay.com/oauth/api_scope https://api.ebay.com/oauth/api_scope/sell.analytics")
        });

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

        _accessToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        return _accessToken;
    }
}
```

### 2. Configure and Use the SDK

```csharp
using EBay.AnalyticsAndReporting;
using EBay.AnalyticsAndReporting.Configuration;

var config = new AnalyticsAndReportingConfiguration
{
    TokenRefreshHandler = new MyTokenRefreshHandler(clientId, secret)
};

using var client = new AnalyticsAndReportingClient(config);

var profiles = await client.FindSellerStandardsProfilesAsync();
var profile = await client.GetSellerStandardsProfileAsync("SELLING_POLICY_COMPLIANCE", "CURRENT");
var metrics = await client.GetCustomerServiceMetricAsync("ITEM_NOT_AS_DESCRIBED", "CURRENT");
var traffic = await client.GetTrafficReportAsync(
    dimension: "DAY",
    filter: "lastUpdatedDate:[2024-01-01T00:00:00.000Z..2024-01-31T23:59:59.999Z]",
    metric: "CLICK_THROUGH_RATE");
```

### 3. Dependency Injection (ASP.NET Core)

`AddAnalyticsAndReportingSdk` registers the configuration and one shared `AnalyticsAndReportingClient` (a singleton that owns its HTTP connections and is disposed with the container). Inject the client and use its properties for the individual APIs:

```csharp
using EBay.AnalyticsAndReporting;
using EBay.AnalyticsAndReporting.Configuration;
using EBay.AnalyticsAndReporting.Extensions;

builder.Services.AddAnalyticsAndReportingSdk(new AnalyticsAndReportingConfiguration
{
    AccessToken = builder.Configuration["EBay:AccessToken"]
});

app.MapGet("/example", async (AnalyticsAndReportingClient sdk) =>
    Results.Ok(await sdk.Analytics.FindSellerStandardsProfilesAsync()));
```

To route requests through your own handler (proxy, client certificates, a test double), pass it to the constructor: `new AnalyticsAndReportingClient(config, handler)`. The handler is shared by all APIs and is not disposed by the client.

## API Operations

#### Find Seller Standards Profiles
```csharp
var profiles = await client.FindSellerStandardsProfilesAsync();
```

#### Get Seller Standards Profile
```csharp
var profile = await client.GetSellerStandardsProfileAsync(
    program: "SELLING_POLICY_COMPLIANCE",
    cycle: "CURRENT"
);
```
**Programs**: `SELLING_POLICY_COMPLIANCE`, etc.  
**Cycles**: `CURRENT`, `PREVIOUS`

#### Get Customer Service Metric
```csharp
var metric = await client.GetCustomerServiceMetricAsync(
    customerServiceMetricType: "ITEM_NOT_AS_DESCRIBED",
    evaluationType: "CURRENT"
);
```
**Metric Types**: `ITEM_NOT_AS_DESCRIBED`, `ITEM_NOT_RECEIVED`  
**Evaluation Types**: `CURRENT`, `PROJECTED`

#### Get Traffic Report
```csharp
var report = await client.GetTrafficReportAsync(
    dimension: "DAY",
    filter: "lastUpdatedDate:[2024-01-01T00:00:00.000Z..2024-12-31T23:59:59.999Z]",
    metric: "CLICK_THROUGH_RATE,LISTING_IMPRESSION_TOTAL",
    sort: "-CLICK_THROUGH_RATE"
);
```
**Dimensions**: `DAY`, `LISTING`  
**Metrics**: `CLICK_THROUGH_RATE`, `LISTING_IMPRESSION_TOTAL`, `LISTING_IMPRESSION_SEARCH_RESULTS_PAGE`, `LISTING_VIEWS_TOTAL`

## Trusted hosts and sandbox

The access token is only sent to trusted hosts: the eBay hosts in `AnalyticsAndReportingConfiguration.DefaultAllowedHosts` (production and their `*.sandbox.ebay.com` counterparts) plus any you add to `AllowedHosts`. The sandbox needs no extra configuration:

```csharp
var config = new AnalyticsAndReportingConfiguration
{
    AccessToken = "...",
    AnalyticsBaseUrl = "https://api.sandbox.ebay.com/sell/analytics/v1",
};
```

Any other host, for example a proxy, has to be added explicitly; the client constructor throws `InvalidOperationException` otherwise:

```csharp
config.AnalyticsBaseUrl = "https://ebay-proxy.example.com/sell/analytics/v1";
config.AllowedHosts.Add("ebay-proxy.example.com");
```

## Error Handling

```csharp
using EBay.AnalyticsAndReporting.Exceptions;

try
{
    var profile = await client.GetSellerStandardsProfileAsync("PROGRAM", "CYCLE");
}
catch (SdkException ex)
{
    Console.WriteLine($"SDK Error: {ex.Message}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.Message}");
}
```

## Regenerating the SDK

When the OpenAPI spec changes:

```bash
./generate.sh

# Or manually:
dotnet tool restore
dotnet kiota generate -l csharp -d specs/openapi/analytics.yaml ...
dotnet build EBay.AnalyticsAndReporting/EBay.AnalyticsAndReporting.csproj
```

## Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `AnalyticsBaseUrl` | `https://api.ebay.com/sell/analytics/v1` | Analytics API base URL |
| `TokenRefreshHandler` | (required) | OAuth token refresh handler |

## Dependencies

- **Microsoft.Kiota.Bundle** (>= 1.22.0): REST runtime
- **Microsoft.Extensions.DependencyInjection.Abstractions** (>= 6.0.0): `AddAnalyticsAndReportingSdk` registration

## License

[Apache 2.0](LICENSE) © 2026 eBay Inc.
