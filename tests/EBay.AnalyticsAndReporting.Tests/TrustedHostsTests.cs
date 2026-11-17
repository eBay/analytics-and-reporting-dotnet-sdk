using EBay.AnalyticsAndReporting.Configuration;
using Xunit;

namespace EBay.AnalyticsAndReporting.Tests;

public sealed class TrustedHostsTests
{
    private const string Sandbox = "https://api.sandbox.ebay.com/sell/analytics/v1";
    private const string Proxy = "https://ebay-proxy.example.com/sell/analytics/v1";

    [Fact]
    public void Untrusted_base_url_is_rejected_by_both_constructors()
    {
        var config = new AnalyticsAndReportingConfiguration { AccessToken = "t", AnalyticsBaseUrl = Proxy };

        var standalone = Assert.Throws<InvalidOperationException>(() => new AnalyticsAndReportingClient(config));
        Assert.Contains("AllowedHosts", standalone.Message);
        Assert.Throws<InvalidOperationException>(() => new TestSdk(configure: c => c.AnalyticsBaseUrl = Proxy));
    }

    [Fact]
    public async Task Production_requests_carry_the_token()
    {
        var sdk = new TestSdk();

        await sdk.Client.Analytics.GetSellerStandardsProfileAsync("CURRENT", "PROGRAM_US");

        Assert.StartsWith("https://api.ebay.com/sell/analytics/v1/", sdk.Handler.RequestUri!.AbsoluteUri);
        Assert.Equal("Bearer config-token", sdk.Handler.Authorization);
    }

    [Fact]
    public async Task Sandbox_hosts_are_trusted_by_default()
    {
        var sdk = new TestSdk(configure: c => c.AnalyticsBaseUrl = Sandbox);

        await sdk.Client.Analytics.GetSellerStandardsProfileAsync("CURRENT", "PROGRAM_US");

        Assert.Equal("api.sandbox.ebay.com", sdk.Handler.RequestUri!.Host);
        Assert.Equal("Bearer config-token", sdk.Handler.Authorization);
    }

    [Fact]
    public async Task Additional_allowed_host_receives_requests_with_token()
    {
        var sdk = new TestSdk(configure: c =>
        {
            c.AnalyticsBaseUrl = Proxy;
            c.AllowedHosts.Add("ebay-proxy.example.com");
        });

        await sdk.Client.Analytics.GetSellerStandardsProfileAsync("CURRENT", "PROGRAM_US");

        Assert.Equal("ebay-proxy.example.com", sdk.Handler.RequestUri!.Host);
        Assert.Equal("Bearer config-token", sdk.Handler.Authorization);
    }

    [Fact]
    public void Default_hosts_are_read_only()
    {
        Assert.Contains("api.ebay.com", AnalyticsAndReportingConfiguration.DefaultAllowedHosts);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)AnalyticsAndReportingConfiguration.DefaultAllowedHosts).Add("evil.example.com"));
    }
}
