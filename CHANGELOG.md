# ChangeLog

## 0.2.0 (09/14/2026)

### Breaking changes

#### Renamed clients and members

| Previous | Current |
| --- | --- |
| `AnalyticsRestSubClient` | `AnalyticsClient` |

#### Changed signatures

`AnalyticsClient`:

| Operation | Previous parameters | Current parameters |
| --- | --- | --- |
| `GetTrafficReportAsync` | `(string? dimension, string? filter, string? metric, string? sort, string? accessToken, RequestOptions? requestOptions, CancellationToken cancellationToken)` | `(string dimension, string filter, string metric, string? sort, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
