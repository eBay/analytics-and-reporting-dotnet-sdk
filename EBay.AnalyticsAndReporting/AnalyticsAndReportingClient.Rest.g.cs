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


using System.Net.Http;
using EBay.AnalyticsAndReporting.Clients.Rest;
using EBay.AnalyticsAndReporting.Configuration;
using EBay.AnalyticsAndReporting.Generated.Analytics;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace EBay.AnalyticsAndReporting;

public partial class AnalyticsAndReportingClient
{
    public AnalyticsRestSubClient Analytics { get; private set; } = null!;

    private void InitializeRestClients(
        IAuthenticationProvider auth,
        HttpClient httpClient,
        AnalyticsAndReportingConfiguration config,
        ITokenContext tokenContext)
    {
        var kiotaClient = BuildKiotaClient(config.AnalyticsBaseUrl, httpClient);
        var adapter = new HttpClientRequestAdapter(auth, httpClient: kiotaClient)
        {
            BaseUrl = config.AnalyticsBaseUrl
        };
        Analytics = new AnalyticsRestSubClient(new AnalyticsApiClient(adapter), config, tokenContext);
    }
}
