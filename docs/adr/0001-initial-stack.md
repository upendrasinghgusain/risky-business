# 0001: Initial stack and structure

- **Status:** Accepted
- **Date:** 2026-10-15

## Context

Stage 1 of the [spec](../spec.md) needs a minimal Angular / ASP.NET Core / relational database setup that runs in Docker Compose locally and on a Raspberry Pi (ARM64). The architecture should be a modular monolith that starts thin and earns its structure.

## Decision

- **Database: PostgreSQL.** SQL Server has no ARM64 Linux container image, so it cannot run on the Pi. PostgreSQL has official multi-arch images, is free and is well supported by EF Core (Npgsql).
- **Backend: .NET 10 (LTS) ASP.NET Core with minimal APIs.** Endpoints are grouped per module with extension methods (e.g. `MapGlossaryEndpoints`).
- **Frontend: Angular with its CLI defaults** (standalone components, signals, Vitest), kept deliberately simple. Node is pinned in `.node-version`; the Angular CLI is a project dependency rather than a global install. In containers it is served by an unprivileged nginx that proxies `/api` to the API, so the browser talks to one origin and no CORS configuration is needed.
- **One API project to begin with.** Modules are folders under `Modules/`; shared persistence lives in `Infrastructure/`. Modules are split into separate projects only once a boundary has proven itself or needs compiler enforcement.
- **Persistence: EF Core with migrations and snake_case naming,** so the schema reads naturally in PostgreSQL tools.
- **Tests: xUnit, with integration tests against real PostgreSQL via Testcontainers** rather than an in-memory substitute.
- **Migrations are applied at API startup** while there is a single instance.

## Consequences

- The Pi is a viable deployment target; images are built for `linux/amd64` and `linux/arm64`.
- Integration tests need Docker wherever they run, including CI.
- Applying migrations at startup is not safe with several API instances and must move to a dedicated step before scale-out (Stage 8), or earlier if a migration needs careful rollout.
- Splitting into multiple projects later is a deliberate refactor, not an accident of the initial layout.
