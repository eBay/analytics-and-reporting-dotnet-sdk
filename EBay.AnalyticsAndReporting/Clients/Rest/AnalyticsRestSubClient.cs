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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EBay.AnalyticsAndReporting.Configuration;
using EBay.AnalyticsAndReporting.Exceptions;
using EBay.AnalyticsAndReporting.Generated.Analytics;
using EBay.AnalyticsAndReporting.Generated.Analytics.Models;
using Microsoft.Kiota.Abstractions;

namespace EBay.AnalyticsAndReporting.Clients.Rest;

public sealed class AnalyticsRestSubClient
{
    private readonly AnalyticsApiClient _kiota;
    private readonly AnalyticsAndReportingConfiguration _config;
    private readonly ITokenContext _tokenContext;

    internal AnalyticsRestSubClient(AnalyticsApiClient kiota, AnalyticsAndReportingConfiguration config, ITokenContext tokenContext)
    {
        _kiota = kiota;
        _config = config;
        _tokenContext = tokenContext;
    }

    /// <summary>
    /// Retrieves a specific seller standards profile by program and cycle.
    /// </summary>
    /// <param name="program">Program name (e.g., PROGRAM_US, PROGRAM_DE, PROGRAM_UK, PROGRAM_GLOBAL)</param>
    /// <param name="cycle">Evaluation cycle (e.g., CURRENT, PROJECTED)</param>
    public async Task<StandardsProfile?> GetSellerStandardsProfileAsync(
        string program,
        string cycle,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _tokenContext.RunAsync(accessToken, () =>
                _kiota.Seller_standards_profile[program][cycle]
                    .GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), cancellationToken));
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedException(ex);
        }
        catch (ApiException ex)
        {
            throw new SdkException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Retrieves all seller standards profiles for the authenticated seller.
    /// </summary>
    public async Task<FindSellerStandardsProfilesResponse?> FindSellerStandardsProfilesAsync(
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _tokenContext.RunAsync(accessToken, () =>
                _kiota.Seller_standards_profile
                    .GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), cancellationToken));
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedException(ex);
        }
        catch (ApiException ex)
        {
            throw new SdkException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Retrieves a seller's performance and rating for the customer service metric.
    /// </summary>
    /// <param name="customerServiceMetricType">Metric type (e.g., ITEM_NOT_AS_DESCRIBED, ITEM_NOT_RECEIVED)</param>
    /// <param name="evaluationType">Evaluation type (e.g., CURRENT, PROJECTED)</param>
    /// <param name="evaluationMarketplaceId">Marketplace ID to evaluate (e.g., EBAY_US, EBAY_GB). Optional.</param>
    public async Task<GetCustomerServiceMetricResponse?> GetCustomerServiceMetricAsync(
        string customerServiceMetricType,
        string evaluationType,
        string? evaluationMarketplaceId = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _tokenContext.RunAsync(accessToken, () =>
                _kiota.Customer_service_metric[customerServiceMetricType][evaluationType]
                    .GetAsync(cfg =>
                    {
                        if (!string.IsNullOrEmpty(evaluationMarketplaceId))
                            cfg.QueryParameters.EvaluationMarketplaceId = evaluationMarketplaceId;
                        ApplyHeaders(cfg.Headers, requestOptions);
                    }, cancellationToken));
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedException(ex);
        }
        catch (ApiException ex)
        {
            throw new SdkException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Retrieves a traffic report showing how buyers engage with a seller's listings.
    /// </summary>
    /// <param name="dimension">Dimension to group by (e.g., DAY, LISTING)</param>
    /// <param name="filter">Filter string for date range, marketplace, and listing IDs</param>
    /// <param name="metric">Comma-separated metrics (e.g., CLICK_THROUGH_RATE, LISTING_IMPRESSION_TOTAL)</param>
    /// <param name="sort">Sort field, prefix with '-' for descending (e.g., -CLICK_THROUGH_RATE)</param>
    public async Task<Report?> GetTrafficReportAsync(
        string? dimension = null,
        string? filter = null,
        string? metric = null,
        string? sort = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _tokenContext.RunAsync(accessToken, () =>
                _kiota.Traffic_report.GetAsync(cfg =>
                {
                    if (!string.IsNullOrEmpty(dimension))
                        cfg.QueryParameters.Dimension = dimension;
                    if (!string.IsNullOrEmpty(filter))
                        cfg.QueryParameters.Filter = filter;
                    if (!string.IsNullOrEmpty(metric))
                        cfg.QueryParameters.Metric = metric;
                    if (!string.IsNullOrEmpty(sort))
                        cfg.QueryParameters.Sort = sort;
                    ApplyHeaders(cfg.Headers, requestOptions);
                }, cancellationToken));
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedException(ex);
        }
        catch (ApiException ex)
        {
            throw new SdkException(ex.Message, ex);
        }
    }

    private void ApplyHeaders(RequestHeaders headers, RequestOptions? callOptions)
    {
        foreach (var (k, v) in _config.DefaultRequestOptions.Headers)
            headers.TryAdd(k, v);
        if (callOptions is null) return;
        foreach (var (k, v) in callOptions.Headers)
            headers[k] = new[] { v };
    }
}
