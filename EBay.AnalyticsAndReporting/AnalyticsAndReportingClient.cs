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
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    // DI constructor — caller manages HttpClient lifetime
    internal AnalyticsAndReportingClient(AnalyticsAndReportingConfiguration config, HttpClient httpClient, ITokenContext tokenContext)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(tokenContext);

        _httpClient = httpClient;
        _ownsHttpClient = false;

        var auth = new BaseBearerTokenAuthenticationProvider(new KiotaAuthProvider(config, tokenContext));
        InitializeRestClients(auth, httpClient, config, tokenContext);
    }

    // Standalone constructor — owns the HttpClient
    public AnalyticsAndReportingClient(AnalyticsAndReportingConfiguration config)
        : this(config, BuildStandaloneHttpClient(config), new AsyncLocalTokenContext())
    {
        _ownsHttpClient = true;
    }

    private static HttpClient BuildStandaloneHttpClient(AnalyticsAndReportingConfiguration config)
    {
        var handler = new DiagnosticLoggingHandler(config)
        {
            InnerHandler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) }
        };
        return new HttpClient(handler) { Timeout = config.HttpTimeout };
    }

    private static HttpClient BuildKiotaClient(string baseUrl, HttpClient httpClient)
    {
        httpClient.BaseAddress = new Uri(baseUrl);
        return httpClient;
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
            _httpClient.Dispose();
    }
}
