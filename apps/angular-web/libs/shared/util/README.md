# shared-util

Shared utilities for the AdventureWorks Angular workspace (`type:util`, `scope:shared`).

## Modules

| Module       | Path                | Description                                                                         |
| ------------ | ------------------- | ----------------------------------------------------------------------------------- |
| Auth         | `lib/auth/`         | `AuthService` (signal-based MSAL wrapper), `AuthUser` model, MSAL factory functions |
| Environment  | `lib/environment/`  | `Environment` interface, `ENVIRONMENT` injection token                              |
| HTTP         | `lib/http/`         | `ApiService`, `correlationIdInterceptor`, `errorInterceptor`                        |
| i18n         | `lib/i18n/`         | `LanguageService` (ngx-translate)                                                   |
| Loading      | `lib/loading/`      | `LoadingService` (ref-counted loading state)                                        |
| Notification | `lib/notification/` | `NotificationService` (signal-based toast queue)                                    |
| Telemetry    | `lib/telemetry/`    | `AppInsightsService` (Azure Application Insights)                                   |
| Theme        | `lib/theme/`        | `ThemeService` (light/dark toggle with localStorage)                                |

## Running unit tests

Run `nx test shared-util` to execute the unit tests.
