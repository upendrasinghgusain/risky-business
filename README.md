# Risky Business

A long-running personal project for learning a slice of commercial insurance by building a real, evolving product with production-grade engineering practices. See [docs/spec.md](docs/spec.md) for the vision, principles and roadmap.

All data is synthetic. Rating factors and rules are invented and labelled as such.

## Repository layout

```text
src/api/RiskyBusiness.Api/        ASP.NET Core API (.NET 10, minimal APIs, EF Core + PostgreSQL)
  Modules/<Module>/               one folder per business module
  Infrastructure/                 DbContext and migrations
src/web/                          Angular SPA, served by nginx in containers
tests/RiskyBusiness.Api.Tests/    unit and integration tests (xUnit, Testcontainers)
docs/                             specification and decision records
compose.yaml                      local stack: PostgreSQL + API + web
```

## Prerequisites

- .NET SDK 10 (pinned in `global.json`)
- Node.js, version pinned in `.node-version` (picked up automatically by fnm with `--use-on-cd`, or run `fnm use`)
- Docker with Compose

## Run the stack

```powershell
Copy-Item .env.example .env   # first time only
docker compose up -d --build
```

- Web app: <http://127.0.0.1:5000>
- API: <http://127.0.0.1:5080>
- Readiness (checks the database): <http://127.0.0.1:5080/health/ready>
- Liveness: <http://127.0.0.1:5080/health/live>

Ports are bound to `127.0.0.1` only. On Windows, use `127.0.0.1` rather than `localhost` to avoid a ~2 s IPv6 fallback delay.

Stop with `docker compose down`. Add `-v` to also delete the database volume.

## Run the API from the IDE

Start just the database with `docker compose up -d db`, then run the API project (`dotnet run --project src/api/RiskyBusiness.Api`). It listens on <http://localhost:5145>, uses the connection string in `appsettings.Development.json`, and applies migrations on startup. [RiskyBusiness.Api.http](src/api/RiskyBusiness.Api/RiskyBusiness.Api.http) has example requests.

## Run the web app with live reload

With the stack running, start the Angular dev server:

```powershell
cd src/web
npm install   # first time, or after package changes
npm start
```

Open <http://localhost:4200>. Requests to `/api` and `/health` are proxied to the API at `127.0.0.1:5080` (see [proxy.conf.json](src/web/proxy.conf.json)). The Angular CLI is a project dependency, so use `npm start` / `npx ng ...` rather than a global `ng`.

## Tests

```powershell
dotnet test
cd src/web; npm test -- --watch=false
```

Integration tests start a disposable PostgreSQL container, so Docker must be running.

## Database migrations

```powershell
dotnet tool restore
dotnet ef migrations add <Name> --project src/api/RiskyBusiness.Api --output-dir Infrastructure/Migrations
```

Migrations are applied when the API starts (`Database:MigrateOnStartup`). This must move to a separate deployment step before running more than one API instance.

## Configuration and secrets

- `.env` (git-ignored) holds Compose secrets; `.env.example` documents them.
- The password in `appsettings.Development.json` and `.env.example` is for local development only and must never be used for a deployed environment.
- In containers, logs are written as JSON to stdout.
