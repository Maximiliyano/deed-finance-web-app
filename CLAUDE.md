# Deed Finance Web App

## Project Overview
Personal finance management app built with Clean Architecture.

## Tech Stack
- **Backend**: ASP.NET Core 9, EF Core 9, MediatR (CQRS), FluentValidation, SQL Server
- **Frontend**: Angular 20, SCSS, Font Awesome 7, Inter font

## Architecture

### Backend (`src/Deed/`)
- `Deed.Domain` — Entities, Enums, Repository interfaces, Result/Error types
- `Deed.Application` — CQRS commands/queries/handlers, validators, specifications, extensions
- `Deed.Infrastructure` — EF Core persistence (DbContext, configurations, repositories, migrations), background jobs
- `Deed.Api` — Minimal API endpoints, middleware, extensions

### Frontend (`src/WebUI/src/app/`)
- `modules/` — Feature modules (home, capital, auth, expense, income)
- `shared/` — Shared components, services, pipes
- `core/` — Layout, guards, interceptors, types

### Tests (`tests/`)
- `Deed.Tests.Unit` — xUnit + NSubstitute + FluentAssertions
- `Deed.Tests.Common` — Shared test utilities (MockHttpMessageHandler)

Follow the structure
// Arrange
// Act
// Assert

## Key Patterns

### Backend
- **CQRS**: Each feature has `Commands/{Create,Update,Delete}/` and `Queries/GetAll/` folders
- **Handlers**: Use primary constructors, inject repository + IUnitOfWork + IUser
- **Specifications**: `BaseSpecification<T>` with expression filters and ordering
- **Extensions**: `ToEntity()`, `ToResponse()`, `ToResponses()`, `ApplyUpdate()` on each entity
- **Result pattern**: All handlers return `Result<T>` or `Result`, check with `IsSuccess`/`Errors`
- **Soft delete**: Entities implement `ISoftDeletableEntity` with `IsDeleted` bool? and query filters
- **Audit**: Entities implement `IAuditableEntity` with `CreatedAt/By`, `UpdatedAt/By`
- **Endpoints**: Minimal API pattern via `IEndpoint.MapEndpoint()`, all require authorization

### Frontend
- Mixed NgModule + standalone components
- Standalone components need explicit `imports: []` (e.g., `CommonModule`)
- CSS custom properties in `:root` — `--primary`, `--bg`, `--surface`, `--border`, `--text`
- Global utility classes: `.btn`, `.card`, `.input`, `.badge`, `.table`, `.empty`, `.page`

### Testing
- Each handler gets its own test class: `{Handler}Tests.cs`
- Mock all dependencies with `Substitute.For<IInterface>()`
- AAA pattern (Arrange/Act/Assert) with comments
- Test both success and failure (NotFound) paths
- Verify repository calls with `.Received(n)` / `.DidNotReceive()`
- Use `[Fact]` for single cases, `[Theory]` + `[InlineData]` for parameterized

## Build & Run

```bash
# Backend
dotnet build src/Deed/Deed.Api

# Frontend
cd src/WebUI && npx ng build

# Tests
dotnet test tests/Deed.Tests.Unit

# Docker (SQL Server + API)
docker compose up -d

# EF Migrations
dotnet ef migrations add <Name> --project src/Deed/Deed.Infrastructure --startup-project src/Deed/Deed.Api --output-dir Persistence/Migrations
```

## Code Style
- File-scoped namespaces (enforced via `.editorconfig`)
- `var` allowed everywhere (IDE0008 disabled)
- `TreatWarningsAsErrors=true` in `Directory.Build.props`
- SonarAnalyzer.CSharp enabled
- All string columns must have explicit `HasMaxLength` in EF configurations
- Audit fields (`CreatedBy`/`UpdatedBy`) capped at `nvarchar(256)`

## Database
- SQL Server 2022 in Docker (port 8002 -> 1433)
- Auto-migrates on startup via `MigrationExtensions.ApplyMigrations()`
- Seed data for Capitals (4) and Categories (16) in Initial migration

## Angular Budget
- `anyComponentStyle`: warning 6kB, error 10kB
- Initial bundle: ~630kB (warning threshold 600kB — not a real issue, ~163kB transferred)
