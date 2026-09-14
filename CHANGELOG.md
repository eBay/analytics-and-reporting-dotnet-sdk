# ChangeLog

## 2.0.0 (09/14/2026)

**Breaking changes**
- The following operations changed signature on `AnalyticsClient`:
  - `GetTrafficReportAsync`: `(string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)` → `(string, string, string, string?, string?, RequestOptions?, CancellationToken)`

## 1.0.0 (09/14/2026)

**Breaking changes**
- The following clients were removed:
  - `AnalyticsRestSubClient`

- The following clients were added:
  - `AnalyticsClient`
