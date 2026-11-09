# Deed Finance

Personal finance management web app: track money across multiple accounts ("capitals") and currencies, log expenses and incomes, plan budgets, set savings goals, track debts and recurring utility bills, and get e-mail reminders.

Built with Clean Architecture (ASP.NET Core 9 + CQRS/MediatR) and an Angular 20 SPA.

<hr />

## Features

| Area | What it does |
| --- | --- |
| **Capitals** | Accounts/wallets with a balance and currency (`UAH`, `USD`, `EUR`, `PLN`, `CZK`). Can be excluded from the total, flagged "only for savings", reordered, sorted and searched. A capital that still has expenses/incomes/transfers cannot be deleted. |
| **Expenses / Incomes** | Records tied to a category and a capital, with purpose, payment date and tags. Expenses are returned grouped by category. Category plans (`PlannedPeriodAmount` per `Day/Week/Month/Year`) are supported. |
| **Categories & Tags** | 16 seeded categories (expense and income types). Categories can be created, edited, soft-deleted and restored. Tags are free-form labels shared by expenses and incomes. |
| **Transfers** | Move money between two capitals, including cross-currency transfers (`Amount` leaves the source, `DestinationAmount` arrives at the destination). |
| **Budget planner** (estimations) | Ordered list of planned purchases with a budget amount/currency that can be linked to a capital and marked completed. |
| **Goals** | Savings goals with target/current amount, optional deadline and completion state. |
| **Debts** | Money borrowed/lent with source, recipient, deadline, optional capital and paid flag. |
| **Utility bills** | Recurring monthly bills (due day of month, estimated amount). "Pay" creates an expense linked to the bill and decreases the capital balance. |
| **Exchange rates** | Daily rates (USD, EUR, PLN, CZK vs. UAH) fetched from the PrivatBank public API, stored in the DB and cached in Redis. A currency-conversion dialog is available in the header. |
| **Bank details** | Reads client info (accounts, jars) from the Monobank personal API using a token from configuration. |
| **User settings & reminders** | Main currency plus optional e-mail reminders (balance summary, expense logging, unpaid debts) on user-defined cron schedules. |
| **Anonymous demo mode** | Most endpoints work without signing in. Anonymous visitors share the `System` user and are limited to **3 created items per entity**; signing in removes the limit. |
| **Theme** | Dark/light theme, remembered in `localStorage` (`deed-theme`) and defaulting to the OS preference. |

## Tech stack

- **Backend:** ASP.NET Core 9 (Minimal APIs), EF Core 9 (SQL Server), MediatR (CQRS), FluentValidation, Quartz.NET (background jobs), Serilog, Polly/`Microsoft.Extensions.Http.Resilience`, Swashbuckle (Swagger), health checks.
- **Infrastructure:** SQL Server 2022, Redis 7 (distributed cache), SMTP (e-mail), Auth0 (OpenID Connect).
- **Frontend:** Angular 20 (NgModules + standalone components), SCSS with CSS custom properties, Tailwind, Font Awesome 7, Inter font, RxJS.
- **Tests:** xUnit, NSubstitute, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`.
- **Delivery:** Docker / Docker Compose, GitHub Actions, Fly.io (API), Netlify (UI).

## Repository layout

```
.
├── src
│   ├── Deed                       # Backend solution (Deed.slnx)
│   │   ├── Deed.Domain            # Entities, enums, repository interfaces, Result/Error types
│   │   ├── Deed.Application       # CQRS commands/queries/handlers, validators, specifications, auth, settings
│   │   ├── Deed.Infrastructure    # EF Core (DbContext, configurations, repositories), Redis, SMTP, Quartz jobs
│   │   └── Deed.Api               # Minimal API endpoints, middleware, DI, Program.cs, Dockerfile
│   └── WebUI                      # Angular SPA (+ nginx.conf, Dockerfile, Netlify proxy function)
├── tests
│   ├── Deed.Tests.Unit            # Handler/validator/job unit tests (CI runs these)
│   ├── Deed.Tests.Integration     # Endpoint tests against a real SQL Server (WebApplicationFactory)
│   └── Deed.Tests.Common          # Shared test helpers (MockHttpMessageHandler)
├── .github/workflows              # backend-deploy, frontend-deploy, claude
├── docker-compose.yml             # SQL Server + Redis + API (+ UI via profiles)
├── fly.toml                       # Fly.io app config for the API
├── Directory.Build.props          # net9.0, nullable, warnings-as-errors, SonarAnalyzer
└── CLAUDE.md                      # Conventions for AI coding assistants
```

### Backend architecture

Dependencies point inwards: `Api -> Infrastructure -> Application -> Domain`.

```
HTTP request
  -> Endpoint (IEndpoint.MapEndpoint, auto-registered by reflection)
  -> MediatR ISender
  -> ValidationPipelineBehavior (FluentValidation) -> RequestLoggingPipelineBehavior
  -> Command/Query handler (repositories + IUnitOfWork + IUser)
  -> Result<T>  ->  ResultExtensions.Process()  ->  200 / 204 / ProblemDetails
```

- Every feature lives in `Deed.Application/<Feature>/` with `Commands/{Create,Update,Delete}`, `Queries/GetAll`, `Requests`, `Responses`, `Specifications` and a `<Feature>Extensions.cs` (`ToEntity`, `ToResponse(s)`, `ApplyUpdate`).
- Queries are expressed as specifications (`BaseSpecification<T>`: criteria, includes, ordering, paging) and evaluated in `Deed.Infrastructure`.
- Handlers never throw for business errors; they return `Result`/`Result<T>` with an `Error` whose `ErrorType` (`Failure`, `Validation`, `BadRequest`, `NotFound`, `Conflict`) maps to an HTTP status (500/400/400/404/409). Unhandled exceptions go through `GlobalExceptionHandler` (RFC 7807 problem details).
- **Soft delete:** entities implement `ISoftDeletableEntity`; a global query filter hides `IsDeleted` rows and `UpdateAuditableEntitiesInterceptor` turns `Delete` into `IsDeleted = true`.
- **Audit:** `IAuditableEntity` (`CreatedAt/By`, `UpdatedAt/By`) is filled by the same interceptor. `CreatedBy` is the OIDC `name` claim (or `System` when anonymous) and is also what scopes each user's data.

### Frontend architecture

- `src/WebUI/src/app/core` - layout (header/footer/error page), guards, HTTP interceptors, shared models/types, theme service.
- `src/WebUI/src/app/modules` - feature modules: `home` (dashboard: budget planner, goals, debts, quick transaction/transfer/salary dialogs), `capital`, `expense` (including utility bills and tag service), `incomes`, `category`, `auth`.
- `src/WebUI/src/app/shared` - reusable components (dialogs, form builder, inputs, icons, currency), pipes and services (notifications, overlays, exchange rates, section loading).
- Routes (lazy-loaded): `/` (home), `/capitals`, `/expenses`, `/incomes`, `/error` (everything else redirects to `/error`).
- HTTP interceptor chain (in order): `credentials` (adds `withCredentials`) -> `retry` (GET only, 2 retries with back-off on network/5xx errors) -> `sectionLoading` (per-section loading flags derived from the URL) -> `error` (401 resets the session, other errors become toast messages).
- `AuthService.me()` calls `/api/users/me`; `login()`/`logout()` navigate to `/api/auth/login` and `/api/auth/logout`.

## Getting started

### Prerequisites

- .NET SDK 9.0 (`src/Deed/global.json`)
- Node.js 20+ and npm
- Docker (for SQL Server and Redis)
- `dotnet-ef` CLI: `dotnet tool install --global dotnet-ef`
- An Auth0 application (domain, client id/secret, API audience) if you want to sign in

### 1. Start dependencies / full backend with Docker

```bash
# SQL Server (host port 8002) + Redis (6379) + API (8000 http / 8001 https)
docker compose up -d

# Full stack with the UI served by nginx on http://localhost:4200
docker compose --profile prod up --build

# Full stack with ng serve (HMR) - see "Known limitations" below
docker compose --profile dev up --build
```

The compose `Development` override mounts your user secrets and dev HTTPS certificates into the API container (`%APPDATA%/Microsoft/UserSecrets`, `%APPDATA%/ASP.NET/Https`).

### 2. Run the API without Docker

Start only the infrastructure (`docker compose up -d deed.db deed.redis`) and point the API at it. `appsettings.Development.json` uses the Docker host name `deed.db`, so override the connection string when running on the host:

```bash
export DatabaseConnection="Server=localhost,8002; Database=DeedDB; User Id=sa; Password=Passw0rd.; TrustServerCertificate=True"
dotnet run --project src/Deed/Deed.Api --launch-profile http
```

- API: `http://localhost:8000`, Swagger UI: `http://localhost:8000/swagger` (Development only).
- Migrations are applied automatically on startup **in Development only**.
- Put Auth0 values in user secrets (`dotnet user-secrets set "AuthSettings:ClientId" "..." --project src/Deed/Deed.Api`) or environment variables (`AuthSettings__ClientId`).

### 3. Run the UI

```bash
cd src/WebUI
npm install
npx ng serve --configuration=development   # http://localhost:4200
```

`proxy.conf.json` forwards `/api` to `http://localhost:8000`, so the SPA and API appear on one origin (needed for the auth cookie).

### Build and test

```bash
dotnet build src/Deed/Deed.Api
dotnet test tests/Deed.Tests.Unit
cd src/WebUI && npx ng build --configuration=production
cd src/WebUI && npx ng lint
```

## Configuration

Backend settings come from `appsettings*.json`, user secrets and environment variables (use `__` as the section separator, e.g. `WebUrlSettings__UIUrl`).

| Key | Purpose |
| --- | --- |
| `DatabaseConnection` | SQL Server connection string. |
| `ConnectionStrings:Redis` | Redis (`host:port` or `redis[s]://:password@host:port`). Default `localhost:6379`; Compose sets `deed.redis:6379`. |
| `AuthSettings:{Domain,ClientId,ClientSecret,Audience}` | Auth0 OpenID Connect settings. |
| `WebUrlSettings:{UIUrl,AllowedHeaders,AllowedMethods}` | Frontend origin used for CORS (credentials allowed) and post-login/logout redirects. |
| `BankSettings:{ExchangeRates,PersonalClientInfo,ApiKey}` | PrivatBank rates URL (`{0}` = `dd.MM.yyyy`), Monobank client-info URL and token. |
| `SmtpSettings:{Host,Port,Username,Password,FromAddress}` | SMTP server for reminders (SSL is always enabled). |
| `BackgroundJobsSettings:{CronExchangeSchedule,CronReminderCheckSchedule}` | Quartz cron expressions for the rates job (default `0 0 9 * * ?`) and the reminder checks (default `0 0/15 * * * ?`). |
| `MemoryCacheSettings:{ExchangesTimespanInHours,CategoriesTimespanInHours}` | Redis TTLs for exchange rates (12h) and categories (1h). |
| `Serilog` | Logging. Development also writes JSON logs to `logs/log-.txt`. |

Never commit real secrets - the values in `appsettings.Development.json` are placeholders.

## Authentication

- Cookie authentication (HttpOnly, `SameSite=Lax`, secure, 1 hour sliding expiration) backed by an Auth0 OpenID Connect challenge (authorization-code flow, scopes `openid profile email offline_access`).
- `GET /api/auth/login?returnUrl=/path` starts the login, `GET /api/auth/logout` clears the cookie and redirects through Auth0's `/v2/logout`, `GET /api/users/me` returns the profile, roles and user settings.
- API calls from the SPA never get redirected to Auth0: a challenge on `/api/*` returns `401` unless it is the explicit login endpoint.
- Only `/api/users/me`, `/api/user-settings` (GET) and creating budget estimations require a signed-in user. All other endpoints are anonymous-accessible and operate on the `System` user until the visitor signs in (see anonymous limits above).
- Data protection keys are persisted in the database (`DataProtectionKeys` table) so cookies survive restarts and multiple instances.

## API overview

All routes are prefixed with `/api`. Request/response bodies are JSON; errors are RFC 7807 problem details with an `errors` array.

| Resource | Routes |
| --- | --- |
| Capitals | `POST /capitals/all` (search/sort/filter), `GET /capitals/{id}`, `POST /capitals`, `PUT /capitals/{id}`, `DELETE /capitals/{id}`, `PUT /capitals/orders`, `PATCH /capitals/{id}/include-in-total`, `PATCH /capitals/{id}/savings-only` |
| Expenses | `GET /expenses` (grouped by category), `GET /expenses/{id}`, `POST /expenses`, `PUT /expenses`, `DELETE /expenses/{id}` |
| Incomes | `GET /incomes`, `GET /incomes/{id}`, `POST /incomes`, `PUT /incomes`, `DELETE /incomes/{id}` |
| Categories | `GET /categories`, `POST /categories`, `PUT /categories/updateRange`, `DELETE /categories/{id}`, `POST /categories/{id}/restore` |
| Tags | `GET /tags`, `POST /tags`, `DELETE /tags/{id}` |
| Transfers | `GET /transfers`, `POST /transfers`, `DELETE /transfers/{id}` |
| Budget estimations | `POST /budget-estimations/all`, `POST /budget-estimations`, `PUT /budget-estimations/{id}`, `PUT /budget-estimations/orders`, `DELETE /budget-estimations/{id}` |
| Goals | `GET /goals`, `POST /goals`, `PUT /goals/{id}`, `PUT /goals/orders`, `DELETE /goals/{id}` |
| Debts | `GET /debts`, `POST /debts`, `PUT /debts/{id}`, `PUT /debts/orders`, `DELETE /debts/{id}` |
| Utility bills | `GET /utility-bills`, `POST /utility-bills`, `PUT /utility-bills/{id}`, `POST /utility-bills/{id}/pay`, `DELETE /utility-bills/{id}` |
| Exchanges / Bank | `GET /exchanges`, `GET /bank/details` |
| User | `GET /auth/login`, `GET /auth/logout`, `GET /users/me`, `GET /user-settings`, `PUT /user-settings` |

Operational endpoints (no `/api` prefix): `GET /health/live` (liveness, no dependency checks) and `GET /health` (SQL Server + Redis checks).

## Background jobs (Quartz.NET)

| Job | Schedule | What it does |
| --- | --- | --- |
| `UpsertLatestExchangeJob` | `CronExchangeSchedule` and once at startup | Fetches rates from PrivatBank, inserts/updates `Exchanges`, clears the Redis exchange cache. |
| `BalanceReminderJob` | every 15 min | For users whose own cron (`BalanceReminderCron`, UTC) fired in the last 15 minutes, e-mails the total balance of "include in total" capitals converted to the user's main currency. |
| `ExpenseReminderJob` | every 15 min | E-mails a reminder to log expenses (`ExpenseReminderCron`). |
| `DebtReminderJob` | every 15 min | E-mails unpaid debts, flagging overdue and due-within-7-days items (`DebtReminderCron`). |

Reminders require `EmailNotificationsEnabled`, an e-mail address and a working `SmtpSettings` section. All jobs use `[DisallowConcurrentExecution]`.

## Testing

- **Unit tests** (`tests/Deed.Tests.Unit`): one test class per handler/validator/job, `Arrange / Act / Assert` comments, `NSubstitute` mocks for all dependencies, `FluentAssertions`. Run with `dotnet test tests/Deed.Tests.Unit`. This is what CI runs.
- **Integration tests** (`tests/Deed.Tests.Integration`): boot the real API through `WebApplicationFactory` with a fake auth handler (user `integration-test-user`), Quartz and health checks removed, and a throwaway `DeedDB_IntegrationTests` database that is dropped, migrated and dropped again by the fixture. They need SQL Server on `localhost,8002` (`docker compose up -d deed.db`) and EF migrations present locally. Not run in CI.
- **Frontend:** Karma/Jasmine is configured (`ng test`), but no spec files exist yet. `ng lint` runs ESLint with the Angular rules.

## CI/CD and hosting

| Part | Hosting | Pipeline |
| --- | --- | --- |
| Backend API | [Fly.io](https://fly.io) app `deed-api` (region `cdg`, rolling deploys, 1 shared CPU / 1 GB, health check `/health/live` on port 8080) | `.github/workflows/backend-deploy.yml`: build + unit tests on PRs and `master`; on `master` runs `flyctl deploy --remote-only` (secret `FLY_API_TOKEN`). |
| Frontend | [Netlify](https://www.netlify.com) | `.github/workflows/frontend-deploy.yml`: `ng build --configuration=production` on PRs and `master`; on `master` publishes `dist/deed/browser` (secrets `NETLIFY_AUTH_TOKEN`, `NETLIFY_SITE_ID`). |
| Database | FreeAspNetHosting (SQL Server) | Connection string supplied through `DatabaseConnection`. |
| AI assistant | GitHub Action `claude.yml` | Responds to `@claude` mentions in issues/PRs (secret `ANTHROPIC_API_KEY`). |

On Netlify the SPA talks to the API through the serverless function `src/WebUI/netlify/functions/api.mts`, which proxies `/api/*`, `/signin-oidc` and `/signout-callback-oidc` to `BACKEND_URL` and adds an `X-Internal-Proxy-Key` header (`PROXY_SECRET`). This keeps the auth cookie first-party to the Netlify domain. `src/WebUI/src/_redirects` provides the SPA fallback. In the nginx image, `/api/` is proxied to `deed.api:8000` instead.

# DB

<h2>Creating migrations</h2>

> **Note:** the `Persistence/Migrations` folder is git-ignored, so a fresh clone has **no migrations**. Generate the initial one locally before running the API (Development applies pending migrations on startup) or the integration tests.

1. To create a migration, open the repository root in a terminal.

1.1. Install EF Core tools for CLI:
'<code>
dotnet tool install --global dotnet-ef
</code>'

2. The next step is to delete the old migrations (if it has the same name of yours), if any, and that is the folder '<b>Migrations</b>' and enter:
'<code>
dotnet ef migrations add Template --startup-project src/Deed/Deed.Api --project src/Deed/Deed.Infrastructure --output-dir Persistence/Migrations
</code>', instead of '<b>Template</b>' specify the desired name. After that, a new folder '<b>Migrations</b>' will appear with the current date and a snapshot of the database.

<h2>DB update via migrations</h2>

'<code>
dotnet ef database update --project src\Deed\Deed.Infrastructure\Deed.Infrastructure.csproj --startup-project src\Deed\Deed.Api\Deed.Api.csproj --context DeedDbContext
</code>'

Seed data (4 capitals from `capitals.json`, 16 categories from `categories.json`) is declared with `HasData` in the entity configurations and is therefore part of the generated migration.

Production does **not** migrate on startup (only Development does), so run `dotnet ef database update` against the production database as part of a release.

<h2>Project overview</h2>

- Backend > fly.io
- Frontend > netlify
- Database > FreeAspNetHosting

# Tests

- Unit tests (xUnit, NSubstitute, FluentAssertions) - `dotnet test tests/Deed.Tests.Unit`
- Integration tests (real SQL Server) - `dotnet test tests/Deed.Tests.Integration`

# API

- CQRS + Mediatr
- Swagger UI in Development: `http://localhost:8000/swagger`

# Web UI

- For local development use <code>ng serve --configuration=development</code>
- Production budgets (`angular.json`): initial bundle warns at 600 kB / fails at 1.5 MB; any component style warns at 16 kB / fails at 24 kB.

## Known limitations

- `docker compose --profile dev` references `src/WebUI/Dockerfile.dev`, which is not in the repository; use `ng serve` on the host instead until it is added.
- `src/Deed/docker-compose*.yml` and `src/Deed/launchSettings.json` are Visual Studio Docker-tooling copies (build context `src/Deed`); the root `docker-compose.yml` is the one to use from the command line.
- The Docker SQL Server password (`Passw0rd.`) is for local development only.
- `SECURITY.md` describes how to report vulnerabilities.
