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
| `GetTrafficReportAsync` | `(string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)` | `(string, string, string, string?, string?, RequestOptions?, CancellationToken)` |
