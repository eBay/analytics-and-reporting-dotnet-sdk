using EBay.AnalyticsAndReporting.Clients.Rest;
using EBay.AnalyticsAndReporting.Configuration;
using EBay.AnalyticsAndReporting.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EBay.AnalyticsAndReporting.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void Registers_one_shared_entry_client_and_no_individual_api_clients()
    {
        var services = new ServiceCollection();
        services.AddAnalyticsAndReportingSdk(new AnalyticsAndReportingConfiguration { AccessToken = "t" });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<AnalyticsAndReportingClient>();

        Assert.Same(client, provider.CreateScope().ServiceProvider.GetRequiredService<AnalyticsAndReportingClient>());
        Assert.NotNull(client.Analytics);
        Assert.Null(provider.GetService<AnalyticsClient>());
    }

    [Fact]
    public void Rejects_a_null_primary_handler()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AnalyticsAndReportingClient(new AnalyticsAndReportingConfiguration(), (HttpMessageHandler)null!));
    }
}
