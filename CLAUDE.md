# Deed Finance Web App

## Project Overview
Personal finance management app built with Clean Architecture.

Domain: multi-currency accounts ("capitals"), expenses/incomes with categories and tags, transfers, budget planner ("estimations"), goals, debts, recurring utility bills, daily exchange rates, e-mail reminders. See `README.md` for the feature list, setup and API overview.

## Tech Stack
- **Backend**: ASP.NET Core 9, EF Core 9, MediatR (CQRS), FluentValidation, SQL Server, Redis (distributed cache), Quartz.NET (jobs), Serilog, Auth0 (OIDC) + cookie auth
- **Frontend**: Angular 20, SCSS, Font Awesome 7, Inter font (Tailwind is also configured)
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
- Global utility classes: `.btn`, `.card`, `.input`, `.badge`, `.table`, `.empty`, `.page`
- Interceptor order (`app.module.ts`): credentials -> retry (GET only) -> sectionLoading -> error (401 => `AuthService.invalidate()`, others => toast)
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
- `UtilityBill` (due day of month, estimated amount); paying it creates an `Expense` with `UtilityBillId` and reduces the capital balance (not allowed for savings-only capitals)
- `Goal`, `Debt`, `BudgetEstimation` have `OrderIndex` for drag-and-drop ordering
- `Exchange` (national/target currency code, `Buy`, `Sale`) is filled from the PrivatBank API; `UserSettings` holds main currency, e-mail and reminder cron expressions
- `CurrencyType`: `None, UAH, USD, EUR, PLN, CZK`. API requests/responses use the string name (`"UAH"`); the DB stores the enum as `int` (`HasConversion<int>()`)

## Auth & anonymous mode
- Cookie auth (HttpOnly, SameSite=Lax, Secure, 1h sliding) + Auth0 OpenID Connect scheme `Auth0` (code flow). Challenges on `/api/*` return 401 unless the explicit `api/auth/login` endpoint is used
- `IUser.Name` is the `name` claim, or `"System"` for anonymous callers. **Per-user data isolation is `CreatedBy == IUser.Name`** in the specifications
- Anonymous visitors share the `System` user and may create at most `AuthConstants.EntityLimit` (3) items per entity (create handlers check `!user.IsAuthenticated`)
- `.RequireAuthorization()`: `api/users/me`, `GET api/user-settings`, `POST api/budget-estimations`, and category writes (create, updateRange, delete, restore) and `POST api/tags`. `GET api/bank/details` and `DELETE api/tags/{id}` need the `AdminOnly` policy (`AuthConstants.AdminOnlyPolicy`, role `admin`). Everything else is anonymous-accessible
- Handlers that touch a `Capital` must look it up with `new CapitalByIdSpecification(id, user.Name)`; an unscoped lookup lets one user change another user's balance
- Policy `AnyUser` is defined but not applied; the roles claim is `https://deed.app/roles` in `AuthClaimTypes` but `https://deed-app/roles` in the OIDC `TokenValidationParameters` (inconsistent)

## Configuration
Settings classes live in `Deed.Application/Abstractions/Settings/` and are bound from the same-named section (`GetRequiredSection`, so each section must exist): `BankSettings`, `WebUrlSettings` (CORS origin + redirects), `BackgroundJobsSettings`, `MemoryCacheSettings`, `AuthSettings`, `SmtpSettings`; plus top-level `DatabaseConnection` and `ConnectionStrings:Redis`. Override with environment variables using `__` (e.g. `AuthSettings__ClientSecret`) or user secrets. `appsettings.Development.json` holds placeholders only; never commit real secrets.

## Background jobs & caching
- Quartz jobs in `Deed.Infrastructure/BackgroundJobs/<Name>/` (`<Name>Job` + `<Name>JobSetup : IConfigureOptions<QuartzOptions>`), all `[DisallowConcurrentExecution]`:
  - `UpsertLatestExchangeJob` — `CronExchangeSchedule` (default 09:00) and once at startup; updates `Exchanges` and evicts `CacheKeys.Exchanges`
  - `BalanceReminderJob`, `ExpenseReminderJob`, `DebtReminderJob` — run every 15 min (`CronReminderCheckSchedule`); send e-mail (SMTP, Polly retry) only for users whose own UTC cron fired in the last 15 min (`CronHelper.HasFiredInWindow`)
- `ICacheService` (Redis via `IDistributedCache`, instance prefix `deed:`) caches exchanges (12h) and categories (1h). Failures are logged and swallowed, so Redis outages fall back to the DB
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
- Netlify function `src/WebUI/netlify/functions/api.mts` proxies `/api/*`, `/signin-oidc`, `/signout-callback-oidc` to `BACKEND_URL` with an `X-Internal-Proxy-Key` header (`PROXY_SECRET`)

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
- `src/Deed/Deed.Infrastructure/Persistence/Migrations` is in `.gitignore`: a fresh clone has no migrations. Create one (see command above) before running the API in Development or the integration tests
- `docker-compose.yml` `dev` profile builds `src/WebUI/Dockerfile.dev`, which does not exist in the repo
- `src/Deed/docker-compose*.yml` and `src/Deed/launchSettings.json` are Visual Studio copies with context `src/Deed`; use the root compose file from the CLI
- `ForwardedHeadersOptions` is configured but `UseForwardedHeaders()` is never called, and the backend does not validate `X-Internal-Proxy-Key`
- `MemoryCacheSettings` is named "Memory" but backs the Redis cache; `appsettings.Testing.json` still uses an old `ExpirationMinutes` key that is not read
