# Deed Finance Web App

## Project Overview
Personal finance management app built with Clean Architecture.

Domain: multi-currency accounts ("capitals"), expenses/incomes with categories and tags, transfers, budget planner ("estimations"), goals, debts, recurring utility bills, daily exchange rates, e-mail reminders. See `README.md` for the feature list, setup and API overview.

## Tech Stack
- **Backend**: ASP.NET Core 10, EF Core 10, MediatR (CQRS), FluentValidation, SQL Server, Redis (distributed cache), Quartz.NET (jobs), Serilog, Auth0 (OIDC) + cookie auth
- **Frontend**: Angular 20, SCSS, Font Awesome 7, Inter font
- **Hosting**: API on Fly.io (`fly.toml`), UI on Netlify, DB on FreeAspNetHosting

## Architecture

### Backend (`src/Deed/`)
- `Deed.Domain` — Entities, Enums, Repository interfaces, Result/Error types
- `Deed.Application` — CQRS commands/queries/handlers, validators, specifications, extensions, auth (`IUser`), settings classes
- `Deed.Infrastructure` — EF Core persistence (DbContext, configurations, repositories, migrations), Redis cache, SMTP e-mail, background jobs
- `Deed.Api` — Minimal API endpoints, middleware, extensions

Dependency direction: `Api -> Infrastructure -> Application -> Domain`. Solution file: `src/Deed/Deed.slnx`.

### Frontend (`src/WebUI/src/app/`)
- `modules/` — Feature modules (home, capital, auth, expense, incomes, category)
- `shared/` — Shared components, services, pipes
- `core/` — Layout, guards, interceptors, types

### Tests (`tests/`)
- `Deed.Tests.Unit` — xUnit + NSubstitute + FluentAssertions
- `Deed.Tests.Integration` — endpoint tests via `WebApplicationFactory` against a real SQL Server (`DeedDB_IntegrationTests`, fake `TestAuthHandler`, Quartz/health checks removed)
- `Deed.Tests.Common` — Shared test utilities (MockHttpMessageHandler)

Follow the structure
// Arrange
// Act
// Assert

## Key Patterns

### Backend
- **CQRS**: Each feature has `Commands/{Create,Update,Delete}/` and `Queries/GetAll/` folders (plus `Requests/`, `Responses/`, `Specifications/`)
- **Handlers**: Use primary constructors, inject repository + IUnitOfWork + IUser
- **Specifications**: `BaseSpecification<T>` with expression filters and ordering
- **Extensions**: `ToEntity()`, `ToResponse()`, `ToResponses()`, `ApplyUpdate()` on each entity
- **Update handlers**: load the tracked entity, return NotFound, then `if (HasNoChanges(command, entity)) return Result.Success();` (a `private static bool HasNoChanges(TCommand, TEntity)` at the bottom of every update handler). A `null` for a nullable entity property clears it (`Purpose`, `Note`); a `null` for a required property or a collection (`TagNames`) means keep. Mutate tracked entities and let `SaveChanges` detect changes; do not call `Update()` on them
- **Tags**: `TagLinkSynchronizer` (Application/Tags) normalizes names, reuses existing `Tag` rows and revives soft-deleted `ExpenseTag`/`IncomeTag` links
- **Queries/performance**: specifications only use `AsSplitQuery` when `UseSplitQuery` is set (several collection includes); never include entities the response does not need. Aggregates are projected in SQL (`ICapitalRepository.GetSummariesAsync` returns `CapitalSummary` with the four totals in one query); existence checks use `HasReferencesAsync` (single `EXISTS`). Category validators use `MustBeCategoryOfType` (one lookup). `UseSqlServer` has `EnableRetryOnFailure` and a 30s command timeout, so do not open manual transactions without an execution strategy
- **Result pattern**: All handlers return `Result<T>` or `Result`, check with `IsSuccess`/`Errors`. `ErrorType` -> HTTP: Failure 500, Validation/BadRequest 400, NotFound 404, Conflict 409 (`ResultExtensions.Process()`)
- **Pipeline**: `ValidationPipelineBehavior` (FluentValidation) then `RequestLoggingPipelineBehavior`; both require `TResponse : Result`
- **Soft delete**: Entities implement `ISoftDeletableEntity` with `IsDeleted` bool and query filters; `UpdateAuditableEntitiesInterceptor` converts `Deleted` into `IsDeleted = true`
- **Audit**: Entities implement `IAuditableEntity` with `CreatedAt/By`, `UpdatedAt/By`, filled by the same interceptor from the `name` claim (`System` when anonymous)
- **Endpoints**: Minimal API pattern via `IEndpoint.MapEndpoint()`, discovered by reflection (`AddEndpoints`/`MapEndpoints`), one class per route under `Deed.Api/Endpoints/<Feature>/`. Routes are `api/<kebab-feature>`; list-with-body queries use `POST .../all`; reordering uses `PUT .../orders`
- **Authorization**: see "Auth & anonymous mode" — most endpoints are `.AllowAnonymous()`, not authenticated-only

### Frontend
- Mixed NgModule + standalone components
- Standalone components need explicit `imports: []` (e.g., `CommonModule`)
- CSS custom properties in `:root` — `--primary`, `--bg`, `--surface`, `--border`, `--text` (dark theme default; `[data-theme="light"]` overrides, toggled by `ThemeService`, persisted as `deed-theme`)
- Global utility classes: `.btn`, `.card`, `.input`, `.badge`, `.table`, `.empty-state`, `.page`
- Interceptor order (`app.module.ts`): credentials -> error (401 => `AuthService.invalidate()`, others => toast) -> retry (GET only) -> sectionLoading. `error` sits outside `retry` so a failing GET toasts once, not per attempt. `credentialsInterceptor` sets `withCredentials`, so services never pass it
- Mobile: dialogs are bottom sheets under 576px, wrap wide tables in `.table-wrap`, inputs are 16px on touch (`pointer: coarse`), cdk drag handles need `touch-action: none`, grids use `minmax(min(Npx, 100%), 1fr)`, fixed bars add `env(safe-area-inset-*)`
- User settings: the API replaces every field, so change a single setting with `UserSettingsService.patch()` (merges into the current settings) instead of `upsert()`
- Budget estimations: `periodEnd` is exclusive (`CreatedAt < periodEnd`); send the start of the next day/month/year
- The home page is a thin shell (`home.component`) that lazy-renders standalone panels with `@defer (on viewport)`: `budget-planner`, `capitals-panel`, `expenses-panel`, `incomes-panel`, `goals-panel`, `debts-panel` (+ presentational `finance-dashboard`). Shared state lives in `DashboardStore` (signals: plan filter, period, capital filter, user currency, exchanges, server `summary`/`chartSummary`); each panel loads its own data. Do not compute currency totals in the frontend: add them to `GET api/dashboard/summary` (handler `GetDashboardSummaryQueryHandler`, `CurrencyConverter`, `IExchangeRateProvider`)
- Frontend conventions: standalone components with `ChangeDetectionStrategy.OnPush`, `inject()` instead of constructor DI, `takeUntilDestroyed(destroyRef)` instead of unsubscribe subjects, services expose `load()` / `refresh()` / `current` / `<entities>$`, shared UI primitives live in `assets/scss/styles.scss` (`.segmented`, `.progress` + `.tone-*`, `.stat-tile`, `.drag-handle`, `.icon-btn`, `.empty-state`, `.section-spinner`, `.toolbar*`, `.dialog-header/body/footer`, `.form-row`, `.table-stack` with `data-label` cells for stacked cards under 576px); spacing/type tokens are `--space-*` / `--fs-*`; breakpoints only via the `respond-to` / `respond-below` / `touch` mixins in `assets/scss/mixins.scss` (no raw media queries, no Tailwind), one sort bar (`app-sort-toolbar`), no code comments. Format with `npm run format` (Prettier, CRLF)
- Routes are lazy-loaded: `/` home, `/capitals`, `/expenses`, `/incomes`, `/error` (wildcard redirects there)
- API calls use relative `/api/...` URLs (dev proxy in `src/WebUI/proxy.conf.json`, nginx `/api/` proxy, Netlify function in prod)

### Testing
- Each handler gets its own test class: `{Handler}Tests.cs`
- Mock all dependencies with `Substitute.For<IInterface>()`
- AAA pattern (Arrange/Act/Assert) with comments
- Test both success and failure (NotFound) paths
- Verify repository calls with `.Received(n)` / `.DidNotReceive()`
- Use `[Fact]` for single cases, `[Theory]` + `[InlineData]` for parameterized
- Validators get `{Command}ValidatorTests.cs`; integration tests derive from `IntegrationTest` (collection `Integration`) and call real routes
- Only unit tests run in CI; integration tests need SQL Server on `localhost,8002`. No Angular `*.spec.ts` files exist yet

## Domain Model
- `Capital` (name, balance, `CurrencyType`, `IncludeInTotal`, `OnlyForSavings`, `OrderIndex`, `RowVersion`) is the hub: `Expense`, `Income`, `Transfer` (source/destination, `Amount` + `DestinationAmount`), `UtilityBill`, optional `Debt`/`BudgetEstimation` link to it. Handlers mutate `Capital.Balance` directly; a capital with references cannot be deleted (`Capital.ReferenceExists` conflict)
- `Category` (`CategoryType`: Expenses/Incomes, `PlannedPeriodAmount` + `TimePeriodType`) is global (not per user) and cached in Redis; `Tag` is linked through `ExpenseTag`/`IncomeTag`
- `UtilityBill` (due day of month, estimated amount); paying it creates an `Expense` with `UtilityBillId` and reduces the capital balance (not allowed for savings-only capitals). A bill with `UnitPrice` (+ optional `Unit`, max 16) is metered: paying takes the current `MeterReading` (stored on the `Expense`, `decimal(18,3)`), and the amount is `(reading - previous) * UnitPrice` in the bill currency, where `previous` is the latest reading paid in the previous UTC calendar month (`IExpenseRepository.GetLatestMeterReadingsAsync`, one batched query; `0` when none, also exposed as `PreviousReading` in `GET api/utility-bills`); a reading not above the previous one fails with `UtilityBill.MeterReadingNotIncreasing`. Non-metered bills are unchanged
- `Goal`, `Debt`, `BudgetEstimation` have `OrderIndex` for drag-and-drop ordering
- `Exchange` (national/target currency code, `Buy`, `Sale`) is filled from the PrivatBank API; `UserSettings` holds main currency, e-mail and reminder cron expressions
- `CurrencyType`: `None, UAH, USD, EUR, PLN, CZK`. API requests/responses use the string name (`"UAH"`); the DB stores the enum as `int` (`HasConversion<int>()`)

## Auth & anonymous mode
- Cookie auth (HttpOnly, SameSite=Lax, Secure, 1h sliding) + Auth0 OpenID Connect scheme `Auth0` (code flow). Challenges on `/api/*` return 401 unless the explicit `api/auth/login` endpoint is used
- `IUser.Name` is the `name` claim, or `"System"` for anonymous callers. **Per-user data isolation is `CreatedBy == IUser.Name`** in the specifications
- Anonymous visitors share the `System` user and may create at most `AuthConstants.EntityLimit` (3) items per entity (create handlers check `!user.IsAuthenticated`)
- `.RequireAuthorization()`: `api/users/me`, `GET api/user-settings`, `POST api/budget-estimations`, `api/bank-connections/*`, `api/bank-notifications/*`, `api/bank-transactions` (paged history), and category writes (create, updateRange, delete, restore) and `POST api/tags`. `GET api/bank/details` and `DELETE api/tags/{id}` need the `AdminOnly` policy (`AuthConstants.AdminOnlyPolicy`, role `admin`). Everything else is anonymous-accessible
- Handlers that touch a `Capital` must look it up with `new CapitalByIdSpecification(id, user.Name)`; an unscoped lookup lets one user change another user's balance
- The roles claim is `AuthSettings:RolesClaim` (default `AuthClaimTypes.Role` = `https://deed-app/roles`); it feeds both the OIDC `RoleClaimType` and `IUser.Roles`, so the Auth0 post-login action must emit exactly that claim name

## Configuration
Settings classes live in `Deed.Application/Abstractions/Settings/` and are bound from the same-named section (`GetRequiredSection`, so each section must exist): `BankSettings`, `WebUrlSettings` (CORS origin + redirects), `BackgroundJobsSettings`, `MemoryCacheSettings`, `AuthSettings`, `SmtpSettings`; `PrivatAutoclientSettings` is optional (defaults to `https://acp.privatbank.ua/api`, path `statements/transactions`; verified by hand: only the final endpoint with `iban=<IBAN>` (not `acc`) returns data, interim and no-`acc` calls return nothing); `MonobankPersonalSettings` is optional too (defaults to `https://api.monobank.ua`; unrelated to the admin-only `BankSettings`); `StatementMailSettings` is optional (`Host`, `Port` 993, `UseSsl`, `UserName`, `Password`, `Address`, `Folder` INBOX, `ProcessedFolder` Processed, `AllowedSenders` `[privatbank.ua]`, `RequireAuthenticationResults` true, `TrustedAuthServId`, `MaxAttachmentBytes`, `MaxMessagesPerRun`; the feature is on only when Host, UserName, Password and an `Address` with `@` are set); plus top-level `DatabaseConnection` and `ConnectionStrings:Redis`. Override with environment variables using `__` (e.g. `AuthSettings__ClientSecret`) or user secrets. `appsettings.Development.json` holds placeholders only; never commit real secrets.

## Background jobs & caching
- Quartz jobs in `Deed.Infrastructure/BackgroundJobs/<Name>/` (`<Name>Job` + `<Name>JobSetup : IConfigureOptions<QuartzOptions>`), all `[DisallowConcurrentExecution]`:
  - `UpsertLatestExchangeJob` — `CronExchangeSchedule` (default 09:00) and once at startup; updates `Exchanges` and evicts `CacheKeys.Exchanges`
  - `BankTransactionSyncJob` — `CronBankSyncSchedule` (default every 5 min); `BankTransactionSyncService` polls the Autoclient API per active `BankConnection` (token encrypted with `ITokenProtector`/DataProtection), dedups by `BankNotification.ExternalId`, writes notifications with an explicit `CreatedBy` (the audit interceptor only fills `CreatedBy` when it is empty). Autoclient field names in `PrivatAutoclientService.Map` come from memory and must be verified against the bank docs. Providers implement `IBankStatementProvider` (Privat Autoclient, Monobank personal API: `X-Token` header, amounts in minor units, one statement request per minute so a 429 maps to `Bank.RateLimited` and the connection is retried on the next run; connections are ordered by `LastSyncAt`). A `BankConnection` can link a `Capital` (+ default expense/income categories): new transactions then get `ImportStatus = Pending` and the user confirms them (`POST api/bank-notifications/{id}/import|dismiss`), or `AutoImport` lets the sync create the `Expense`/`Income` through `IBankTransactionImporter` (balance changes, currency must match the capital; history backfill stays `None`)
  - `BankBackfillJob` — `CronBankBackfillSchedule` (default every minute); `POST api/bank-connections/history {from, to?}` (up to ~10 years; an empty chunk ends the walk once it lies more than 400 days before the oldest stored transaction) stores `BackfillFrom`/`BackfillCursor` on each connection and imports the newest 30-day chunk immediately, then the job walks the cursor backwards: one chunk per run for Monobank, `ChunksPerRun` (default 6, `ChunkDelayMs` 1s apart, stops on a rate limit or failure) for Privat; starting an import or connecting an account triggers the job at once through `IBankBackfillTrigger` (Quartz `TriggerJob`) (Monobank: 1 request/min, 500 items per statement, so a full page resumes from its oldest item). A failed chunk is retried on the next run (rate limits do not count); ten consecutive failures stop the import, `DELETE api/bank-connections/history` cancels it. Privat only returns closed days from `statements/transactions`, so the newest day can be missing until the bank closes it. History rows are staged read-only (`ImportStatus = None`)
  - `BankAccountRefreshJob` — `CronBankAccountRefreshSchedule` (default daily 05:00 UTC) and once at startup; `BankAccountRefreshService` calls the Autoclient balance endpoint once per distinct token and upserts `BankAccount` rows (IBAN, currency, name, balance, `SourceConnectionId`). Accounts without a `BankConnection` are only listed as available (`GET api/bank-accounts`, `POST api/bank-accounts/refresh`); `POST api/bank-accounts/{id}/connect` creates a connection reusing the source connection's encrypted token. Response field names of the balance endpoint (`balances[]`: `acc`, `currency`, `nameACC`, `balanceOut`) are from memory; on no match the service logs the JSON shape
  - `StatementMailSyncJob` — `CronStatementMailSchedule` (default every 10 min); personal Privat24 cards have no live API, so a `BankProvider.PrivatMail` connection (`POST api/bank-connections/privat-mail {iban, label?, currency?}`, no token: `EncryptedToken` is empty, `ProviderAccountId` holds a random 16-hex routing key, no schema change) gets a private address `local+<key>@domain` built from `StatementMailSettings:Address` (`GET api/bank-connections/mail-info` returns `{enabled, addressTemplate}` with `{key}`; `BankConnectionResponse.MailKey` carries the key). The job reads UNSEEN mail over IMAP (MailKit), `StatementMailImportService` accepts a message only when the From domain is in `AllowedSenders` and the topmost `Authentication-Results` has `dkim=pass` (header.d/i) or `spf=pass` (smtp.mailfrom) for an allowed domain (`StatementMailPolicy`), finds the connection by the key in To/Cc/Delivered-To/X-Original-To (`StatementMailAddress.ExtractKeys`, never by IBAN alone, so nobody can feed another user's account), skips a statement that names a different IBAN, parses csv/tsv/txt (utf-8, utf-16, windows-1251), html tables and xlsx (built-in zip+xml, no package; legacy binary .xls is unsupported) with header aliases (`StatementColumns`), and hands the rows to `IBankTransactionSyncService.ImportStatementAsync` (rows before `CreatedAt - 10 min` are staged as history with `ImportStatus = None`, newer ones follow Pending/AutoImport). `ExternalId` = `mail:` + SHA-256 prefix of iban, instant, signed amount, currency, text and balance, plus `-n` for identical rows inside one statement, so re-sent statements never duplicate. Handled mail is flagged seen and moved to `ProcessedFolder`; an exception leaves it unseen for the next run. Mail connections are excluded from `ActiveBankConnectionsSpecification` (no pull sync, no 'Check now', no backfill). The real Privat24 statement e-mail layout is unverified: the column aliases come from memory, so check the logged 'Statement file ... columns [...]' line against a real e-mail and adjust `StatementColumns` if a column is not detected.
  - Sync cadence: `IBankStatementProvider.MinSyncInterval` (Privat 30 min via `PrivatAutoclientSettings:SyncIntervalMinutes`, Monobank 0) makes the 5-minute job skip fresh connections; `SyncUserAsync` ("Check now") ignores it. The backfill re-reads `BankConnectionRepository.GetBackfillStateAsync` before writing a chunk, so a cancel or exclusion that lands mid-run wins (no concurrency token, because it would make the sync job fail). The Privat HTTP client does not retry 429
  - Data protection: `DataProtectionSettings` (optional) protects the key ring with a certificate; `KeyRingProtectionService` warns when it is missing and, with `ReprotectExistingKeys`, re-encrypts every `BankConnection.EncryptedToken` and deletes plain-text keys (signs users out once)
  - Schema notes: no foreign keys on `BankNotification.BankConnectionId` / `BankAccount.SourceConnectionId` (connections are soft-deleted, and legacy rows have no connection); `BankConnections` has a unique filtered index on `(CreatedBy, Provider, AccountIban)` where not deleted
  - `BankConnection.Currency` is the currency of the account (set when connecting, refreshed with the account list). Privat statements put the amount (`SUM`) in the account currency while `CCY` can be the merchant's currency (a EUR card payment of UAH 382 comes as `SUM 382`, `CCY EUR`), so sync and history overwrite the statement currency with the connection's; the `AddBankConnectionCurrency` migration repairs stored rows from `BankAccounts`. Monobank statements behave the same (`currencyCode` is the transaction currency, `amount` the account's): the connect dialog passes the account currency, and older Monobank connections learn it once from `client-info` (`LearnCurrencyAsync`, which skips that run's statement request because of the 1 request/min limit) and repair their stored rows
  - `BankConnection.IsExcluded` ("not counted", `PUT api/bank-connections/{id}/excluded`) keeps the connection and its token but skips sync/backfill and hides its transactions from the bank page (`BankConnectionScope`); `DELETE api/bank-connections/provider/{provider}` removes every connection, `BankAccount` and stored `BankNotification` of that bank. Connections with an empty `AccountIban` (old "all accounts" mode) are deleted by the `AddBankConnectionExcluded` migration
  - `ReminderDigestJob` — runs every 15 min (`CronReminderCheckSchedule`); for each user it collects the sections whose own UTC cron fired in the last 15 min (`CronHelper`) and `ReminderDigestService` sends them as ONE e-mail: balance (`BalanceReminderEnabled`), expenses and incomes since the previous fire plus bank transactions when `BankAlertsEmailEnabled` (`ExpenseReminderEnabled`), unpaid debts (`DebtReminderEnabled`)
- `ICacheService` (Redis via `IDistributedCache`, instance prefix `deed:`) caches exchanges (12h) and categories (1h). Category keys come from `CacheKeys.Category(type, includeDeleted)`; every category write (create, delete, restore, updateRange) calls `cache.RemoveCategoriesAsync()` after saving. Failures are logged and swallowed, so Redis outages fall back to the DB
- `ExchangeHttpService` uses the standard resilience handler and keeps only USD, EUR, PLN, CZK

## API Endpoints (summary)
Feature routes under `api/`: `capitals`, `expenses`, `incomes`, `categories`, `tags`, `transfers`, `budget-estimations`, `goals`, `debts`, `utility-bills`, `exchanges`, `bank/details`, `user-settings`, `users/me`, `auth/login`, `auth/logout`. Health: `/health/live` (no checks), `/health` (SQL + Redis). Swagger only in Development. Full table in `README.md`.

## Build & Run

```bash
# Backend
dotnet build src/Deed/Deed.Api

# Frontend
cd src/WebUI && npx ng build

# Tests
dotnet test tests/Deed.Tests.Unit
dotnet test tests/Deed.Tests.Integration   # needs SQL Server on localhost,8002 + local migrations

# Docker (SQL Server + API + Redis only)
docker compose up -d

# Docker (full stack — nginx-served Angular + API + DB + Redis)
docker compose --profile prod up --build

# Docker (full stack — ng serve HMR + API + DB + Redis)
docker compose --profile dev up --build

# EF Migrations
dotnet ef migrations add <Name> --project src/Deed/Deed.Infrastructure --startup-project src/Deed/Deed.Api --output-dir Persistence/Migrations
```

Ports: API 8000 (http) / 8001 (https) locally, 8080 on Fly.io; SQL Server 8002 -> 1433; Redis 6379; UI 4200. Running the API on the host needs `DatabaseConnection` overridden (Development config uses the Docker host `deed.db`), e.g. `Server=localhost,8002; Database=DeedDB; User Id=sa; Password=Passw0rd.; TrustServerCertificate=True`.

## CI/CD
- `.github/workflows/backend-deploy.yml`: build + unit tests (PRs and `master`), then `flyctl deploy --remote-only` on `master` (`FLY_API_TOKEN`)
- `.github/workflows/frontend-deploy.yml`: `ng build --configuration=production` (Node 20), then Netlify deploy on `master` (`NETLIFY_AUTH_TOKEN`, `NETLIFY_SITE_ID`)
- `.github/workflows/claude.yml`: `@claude` mentions in issues/PRs (`ANTHROPIC_API_KEY`)
- Netlify function `src/WebUI/netlify/functions/api.mts` proxies `/api/*`, `/signin-oidc`, `/signout-callback-oidc` to `BACKEND_URL` with an `X-Internal-Proxy-Key` header (`PROXY_SECRET`) and the real client IP in `X-Client-Ip`. `ProxyKeyMiddleware` (`ProxySettings:Key`) returns 403 without the key (off when the key is empty; `/health/live` is exempt), `UseForwardedHeaders()` runs first, `SecurityHeadersMiddleware` is production-only, and `RateLimitingExtensions` partitions by `name` claim or client IP (policies `bank`, `auth` via `RequireRateLimiting`; 429 maps to `ErrorType.TooManyRequests`)

## Code Style
- File-scoped namespaces (enforced via `.editorconfig`)
- `var` allowed everywhere (IDE0008 disabled)
- `TreatWarningsAsErrors=true` in `Directory.Build.props` (also `AnalysisMode=all`, `EnforceCodeStyleInBuild`)
- SonarAnalyzer.CSharp enabled
- All string columns must have explicit `HasMaxLength` in EF configurations
- Audit fields (`CreatedBy`/`UpdatedBy`) capped at `nvarchar(256)`
- Preserve each file's existing line endings and BOM when editing (the `.md` files are CRLF, UTF-8 without BOM)

## Database
- SQL Server 2022 in Docker (port 8002 -> 1433)
- Auto-migrates on startup via `MigrationExtensions.ApplyMigrations()` **in Development only**; production must run `dotnet ef database update` manually
- Seed data for Capitals (4) and Categories (16) comes from `Persistence/DataSeed/*.json` via `HasData`, so it ends up in the generated Initial migration
- DataProtection keys are stored in the DB (`DeedDbContext : IDataProtectionKeyContext`)

## Angular Budget
- `anyComponentStyle`: warning 16kB, error 24kB (`angular.json`)
- `initial`: warning 600kB, error 1.5MB
- Initial bundle: ~630kB (warning threshold 600kB — not a real issue, ~163kB transferred)

## Gotchas
- The API `Dockerfile` publishes ReadyToRun for `linux-x64`: `dotnet restore` must also get `-r linux-x64 /p:PublishReadyToRun=true`, otherwise the later `publish --no-restore` fails with NETSDK1094
- `fly.toml` keeps one machine running (`min_machines_running = 1`) so requests do not hit a cold start and the Quartz jobs keep ticking
- `src/Deed/Deed.Infrastructure/Persistence/Migrations` is in `.gitignore`: a fresh clone has no migrations. Create one (see command above) before running the API in Development or the integration tests
- `docker-compose.yml` `dev` profile builds `src/WebUI/Dockerfile.dev`, which does not exist in the repo
- `src/Deed/docker-compose*.yml` and `src/Deed/launchSettings.json` are Visual Studio copies with context `src/Deed`; use the root compose file from the CLI
- `ForwardedHeadersOptions` trusts any proxy (`KnownProxies` cleared), so the proxy key is what keeps forwarded headers honest: leave `ProxySettings:Key` set in production
- `MemoryCacheSettings` is named "Memory" but backs the Redis cache
