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
using EBay.AnalyticsAndReporting.Clients.Rest;
using EBay.AnalyticsAndReporting.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBay.AnalyticsAndReporting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyticsAndReportingSdk(
        this IServiceCollection services,
        AnalyticsAndReportingConfiguration config)
    {
        services.AddSingleton(config);
        services.AddSingleton<ITokenContext, AsyncLocalTokenContext>();

        services.AddHttpClient(AnalyticsAndReportingClient.AnalyticsClientName,
                (sp, c) => c.BaseAddress = new Uri(sp.GetRequiredService<AnalyticsAndReportingConfiguration>().AnalyticsBaseUrl))
            .ConfigureHttpClient((sp, c) => c.Timeout = sp.GetRequiredService<AnalyticsAndReportingConfiguration>().HttpTimeout)
            .AddHttpMessageHandler(sp => new DiagnosticLoggingHandler(sp.GetRequiredService<AnalyticsAndReportingConfiguration>()));

        services.AddScoped<AnalyticsAndReportingClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new AnalyticsAndReportingClient(
                sp.GetRequiredService<AnalyticsAndReportingConfiguration>(),
                factory.CreateClient(AnalyticsAndReportingClient.AnalyticsClientName),
                sp.GetRequiredService<ITokenContext>());
        });

        services.AddScoped(sp => sp.GetRequiredService<AnalyticsAndReportingClient>().Analytics);

        return services;
    }
}
