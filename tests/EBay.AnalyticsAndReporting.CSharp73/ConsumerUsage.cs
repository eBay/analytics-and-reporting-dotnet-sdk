using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EBay.AnalyticsAndReporting;
using EBay.AnalyticsAndReporting.Configuration;
using EBay.AnalyticsAndReporting.Exceptions;

namespace EBay.AnalyticsAndReporting.CSharp73
{
    /// <summary>Typical consumer code written in C# 7.3; it only has to compile.</summary>
    public static class ConsumerUsage
    {
        public static async Task<string> RunAsync()
        {
            var config = new AnalyticsAndReportingConfiguration
            {
                AccessToken = "token",
                HttpTimeout = TimeSpan.FromSeconds(60),
                DiagnosticLogging = true,
                AnalyticsBaseUrl = "https://ebay-proxy.example.com/sell/analytics/v1",
                AllowedHosts = { "ebay-proxy.example.com" },
                DefaultRequestOptions = new RequestOptions
                {
                    Headers = new Dictionary<string, string> { { "X-EBAY-C-MARKETPLACE-ID", "EBAY_US" } }
                }
            };

            using (var client = new AnalyticsAndReportingClient(config))
            {
                try
                {
                    var result = await client.Analytics.GetSellerStandardsProfileAsync("CURRENT", "PROGRAM_US");
                    return result != null ? "ok" : "empty";
                }
                catch (UnauthorizedException)
                {
                    return "unauthorized";
                }
            }
        }
    }
}
