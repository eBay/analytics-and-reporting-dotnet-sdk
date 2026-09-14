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

using EBay.AnalyticsAndReporting.Configuration;
using EBay.AnalyticsAndReporting.Generated.Analytics.Models;
using EBay.AnalyticsAndReporting.Generated.Analytics;
using Microsoft.Kiota.Abstractions;

namespace EBay.AnalyticsAndReporting.Clients.Rest;

public sealed class AnalyticsClient
{
    private readonly AnalyticsApiClient _kiota;
    private readonly AnalyticsAndReportingConfiguration _config;
    private readonly ITokenContext _tokenContext;
    private readonly IRequestAdapter _adapter;

    internal AnalyticsClient(AnalyticsApiClient kiota, AnalyticsAndReportingConfiguration config, ITokenContext tokenContext, IRequestAdapter adapter)
    {
        _kiota = kiota;
        _config = config;
        _tokenContext = tokenContext;
        _adapter = adapter;
    }

    /// <summary>
    /// Use this method to retrieve a seller's performance and rating for the customer service metric.
    /// Control the response from the <b>getCustomerServiceMetric</b> method using the following path and query parameters:
    /// - <b>customer_service_metric_type</b> controls the type of customer service transactions evaluated for the metric rating.
    /// - <b>evaluation_type</b> controls the period you want to review.
    /// - <b>evaluation_marketplace_id</b> specifies the target marketplace for the evaluation.
    /// Currently, metric data is returned for only peer benchmarking. For details on the workings of peer benchmarking, see <see href="https://www.ebay.com/help/policies/selling-policies/seller-performance-policy/service-metrics-policy?id=4769">Service metrics policy</see>.
    /// For details on using and understanding the response from this method, see <see href="/api-docs/sell/static/performance/customer-service-metric.html">Interpreting customer service metric ratings</see>.
    /// </summary>
    public Task<GetCustomerServiceMetricResponse?> GetCustomerServiceMetricAsync(
        string customerServiceMetricType,
        string evaluationMarketplaceId,
        string evaluationType,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Customer_service_metric[customerServiceMetricType][evaluationType].GetAsync(cfg =>
        {
            cfg.QueryParameters.EvaluationMarketplaceId = evaluationMarketplaceId;
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves all the standards profiles for the associated seller.
    /// A <i>standards profile </i> is a set of eBay seller metrics and the seller's associated compliance values (either <c>TOP_RATED</c>, <c>ABOVE_STANDARD</c>, or <c>BELOW_STANDARD</c>).
    /// A seller's multiple profiles are distinguished by two criteria, a "program" and a "cycle." A profile's <i>program </i> is one of three regions where the seller may have done business, or <c>PROGRAM_GLOBAL</c> to indicate all marketplaces where the seller has done business. The <i>cycle</i> value specifies whether the standards compliance values were determined at the last official eBay evaluation or at the time of the request.
    /// </summary>
    public Task<FindSellerStandardsProfilesResponse?> FindSellerStandardsProfilesAsync(
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Seller_standards_profile.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves a single standards profile for the associated seller.
    /// A <i>standards profile </i> is a set of eBay seller metrics and the seller's associated compliance values (either <c>TOP_RATED</c>, <c>ABOVE_STANDARD</c>, or <c>BELOW_STANDARD</c>).
    /// A seller can have multiple profiles distinguished by two criteria, a "program" and a "cycle." A profile's <i>program </i> is one of three regions where the seller may have done business, or <c>PROGRAM_GLOBAL</c> to indicate all marketplaces where the seller has done business. The <i>cycle</i> value specifies whether the standards compliance values were determined at the last official eBay evaluation (<c>CURRENT</c>) or at the time of the request (<c>PROJECTED</c>). Both cycle and a program values are required URI parameters for this method.
    /// </summary>
    public Task<StandardsProfile?> GetSellerStandardsProfileAsync(
        string cycle,
        string program,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Seller_standards_profile[program][cycle].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method returns a report that details the user traffic received by a seller's listings.
    /// A traffic report gives sellers the ability to review how often their listings appeared on eBay, how many times their listings are viewed, and how many purchases were made. The report also returns the report's start and end dates, and the date the information was last updated.
    /// For more information, see <see href="/api-docs/sell/static/performance/traffic-report.html">Traffic report details</see>
    /// </summary>
    public Task<Report?> GetTrafficReportAsync(
        string? dimension = null,
        string? filter = null,
        string? metric = null,
        string? sort = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Traffic_report.GetAsync(cfg =>
        {
            if (dimension is not null)
            {
                cfg.QueryParameters.Dimension = dimension;
            }
            if (filter is not null)
            {
                cfg.QueryParameters.Filter = filter;
            }
            if (metric is not null)
            {
                cfg.QueryParameters.Metric = metric;
            }
            if (sort is not null)
            {
                cfg.QueryParameters.Sort = sort;
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));
}
