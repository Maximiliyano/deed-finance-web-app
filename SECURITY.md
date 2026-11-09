# Security Policy

## Supported Versions

Deed Finance does not publish tagged or numbered releases. The `master` branch is built and deployed continuously (API to Fly.io, UI to Netlify), so only the latest `master` commit is supported with security fixes.

| Version                 | Supported          |
| ----------------------- | ------------------ |
| latest `master` (deployed) | :white_check_mark: |
| older commits / forks   | :x:                |

## Reporting a Vulnerability

Please do **not** open a public issue for security problems.

1. Use GitHub's private reporting: go to the repository's **Security** tab and choose **Report a vulnerability** (<https://github.com/Maximiliyano/deed-finance-web-app/security/advisories/new>).
2. If private reporting is not available, contact the repository owner ([@Maximiliyano](https://github.com/Maximiliyano)) through GitHub and share details privately.

Please include:

- a description of the issue and its impact,
- the affected endpoint, page or component (for example `api/capitals`, the Netlify proxy function, the Auth0 login flow),
- steps to reproduce or a proof of concept,
- any suggested fix.

This is a personal project maintained on a best-effort basis. Reports are acknowledged and triaged as soon as the maintainer is able to; accepted vulnerabilities are fixed on `master` and deployed, and declined reports receive an explanation.

## Scope and Security Notes

Areas that are in scope:

- **Authentication and sessions** - Auth0 OpenID Connect with an HttpOnly, `SameSite=Lax`, secure cookie (1 hour sliding expiration); data protection keys are stored in the database.
- **Authorization and data isolation** - user data is scoped by the `CreatedBy` value (the OIDC `name` claim). Most API endpoints also accept anonymous callers, who share a single `System` user and are limited to 3 created items per entity.
- **API surface** - ASP.NET Core Minimal API (`/api/*`), CORS restricted to the configured `WebUrlSettings:UIUrl` with credentials, FluentValidation on commands.
- **Netlify proxy** - `src/WebUI/netlify/functions/api.mts` forwards `/api/*`, `/signin-oidc` and `/signout-callback-oidc` to the backend and adds an `X-Internal-Proxy-Key` header.
- **Third-party integrations** - PrivatBank rates API, Monobank personal API token (`BankSettings:ApiKey`) and SMTP credentials.

Secrets (Auth0 client secret, Monobank token, SMTP password, database connection string, `FLY_API_TOKEN`, `NETLIFY_AUTH_TOKEN`, `PROXY_SECRET`, `ANTHROPIC_API_KEY`) must be supplied through environment variables, user secrets or CI secrets and must never be committed. The credentials in `docker-compose.yml` and `appsettings.Development.json` (for example the SQL Server `sa` password) are placeholders for local development only. If you find a real secret in the repository or its history, please report it using the process above.
