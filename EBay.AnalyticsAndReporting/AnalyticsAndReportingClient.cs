// Copyright 2026 eBay Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using System;
using System.Net.Http;
using EBay.AnalyticsAndReporting.Configuration;
using Microsoft.Kiota.Abstractions.Authentication;

namespace EBay.AnalyticsAndReporting;

public partial class AnalyticsAndReportingClient : IDisposable
{
    // Named client keys used by ServiceCollectionExtensions
    internal const string AnalyticsClientName = "AnalyticsAndReportingClient.Analytics";

    private readonly HttpClient[] _ownedClients;

    /// <summary>
    /// DI-friendly constructor: each HttpClient is a named client from IHttpClientFactory
    /// with DiagnosticLoggingHandler and BaseAddress already configured.
    /// </summary>
    internal AnalyticsAndReportingClient(
        AnalyticsAndReportingConfiguration config,
        HttpClient analyticsClient,
        ITokenContext tokenContext)
    {
        _ownedClients = [];

        var auth = new BaseBearerTokenAuthenticationProvider(new KiotaAuthProvider(config, tokenContext));

        InitializeRestClients(auth, analyticsClient, config, tokenContext);
    }

    /// <summary>
    /// Standalone constructor for use outside DI (tests, scripts).
    /// Creates its own HttpClients with DiagnosticLoggingHandler.
    /// </summary>
    public AnalyticsAndReportingClient(AnalyticsAndReportingConfiguration config)
    {
        var tokenContext = new AsyncLocalTokenContext();
        var auth = new BaseBearerTokenAuthenticationProvider(new KiotaAuthProvider(config, tokenContext));

        var analyticsClient = BuildStandaloneHttpClient(config, config.AnalyticsBaseUrl);
        _ownedClients = [analyticsClient];

        InitializeRestClients(auth, analyticsClient, config, tokenContext);
    }

    private static HttpClient BuildStandaloneHttpClient(AnalyticsAndReportingConfiguration config, string baseUrl)
    {
        var handler = new DiagnosticLoggingHandler(config)
        {
            InnerHandler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) }
        };
        return new HttpClient(handler) { BaseAddress = new Uri(baseUrl), Timeout = config.HttpTimeout };
    }

    public void Dispose()
    {
        foreach (var client in _ownedClients)
            client.Dispose();
    }
}
