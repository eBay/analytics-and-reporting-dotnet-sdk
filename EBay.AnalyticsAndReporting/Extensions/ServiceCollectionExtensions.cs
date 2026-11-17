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

using EBay.AnalyticsAndReporting;
using EBay.AnalyticsAndReporting.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBay.AnalyticsAndReporting.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <paramref name="config"/> and a singleton <see cref="AnalyticsAndReportingClient"/>. The client
    /// owns its HTTP connections and is disposed with the container. Reach the individual APIs through
    /// its properties.
    /// </summary>
    public static IServiceCollection AddAnalyticsAndReportingSdk(
        this IServiceCollection services,
        AnalyticsAndReportingConfiguration config)
    {
        services.AddSingleton(config);
        services.AddSingleton(_ => new AnalyticsAndReportingClient(config));
        return services;
    }
}
