# Containerized Local Development

Run the AdventureWorks API and Angular web app locally with Docker. The database is managed separately — point the API at your existing SQL Server instance.

## Prerequisites

- **Docker Desktop 4.30+** (or compatible Docker Engine + Compose V2)
- **4 GB RAM** allocated to Docker
- **Ports available:** 5000 (API), 4200 (Web)
- **SQL Server** with the AdventureWorks database already set up and accessible

## Quick Start

```bash
# 1. Create your env file from the example
cp .env.docker.example .env.docker

# 2. Edit CONNECTION_STRING to point to your SQL Server
#    Use host.docker.internal to reach your host machine from inside Docker

# 3. Build and start the API and web app
docker compose --env-file .env.docker up --build
```

First build takes several minutes (NuGet restore, npm ci, Angular build). Subsequent builds use Docker layer caching.

## Service URLs

| Service         | URL                          | Description                   |
| --------------- | ---------------------------- | ----------------------------- |
| Web App         | http://localhost:4200        | Angular app (nginx)           |
| API (via nginx) | http://localhost:4200/api/   | Proxied through web container |
| API (direct)    | http://localhost:5000        | .NET API direct access        |
| API Health      | http://localhost:5000/health | Health check endpoint         |

## Teardown

```bash
docker compose --env-file .env.docker down
```

## Service Architecture

```
                  +-----------+
  :4200 -------->|   nginx   |---- /api/ ---->+-----------+
                  | (Angular) |               |  .NET API |---- :8080
                  +-----------+               +-----------+
                                                    |
                                              +-----------+
                                              | SQL Server| (external, not containerized)
                                              +-----------+
```

**Startup order:** API (healthy) -> Web

## Database Backup Directory (DbReset.Console)

The `tosk-mssql` SQL Server container stores backup files at this container-internal path:

```
/var/opt/mssql/backup/
```

The host path for the same directory (OrbStack) is:

```
~/OrbStack/docker/volumes/sql_tosk_mssql_2025_data/backup/
```

(OrbStack only — for Docker Desktop, find the volume in Docker Desktop's Volumes tab.)

**One-time permission setup** — SQL Server runs as the `mssql` user (uid 10001) and cannot read files owned by root. Run this once after the container is first created, and again after each `snapshot` that writes new files:

```bash
docker exec --user root tosk-mssql chmod -R o+r /var/opt/mssql/backup/
```

**Target databases are auto-created on first restore** — `AdventureWorks_E2E` and any other target matching `DbReset:TargetNamePattern` do not need to be pre-created in SQL Server. The `restore` verb creates the database from the `.bak` automatically.

See [tools/console-apps/AdventureWorks.DbReset.Console/README.md](tools/console-apps/AdventureWorks.DbReset.Console/README.md) for full setup and Getting Started instructions.

## Troubleshooting

### Port conflicts

If ports 5000 or 4200 are in use, change the host port mapping in `docker-compose.yml` (e.g., `"5001:8080"`).

### API cannot reach SQL Server

Use `host.docker.internal` as the server name in your connection string to reach your host machine's SQL Server from inside the container. Ensure SQL Server is configured to accept TCP/IP connections.

### Apple Silicon (ARM64)

The .NET and nginx images support ARM64 natively. No Rosetta emulation needed.

### Out of memory

If containers crash, increase Docker Desktop memory allocation to at least 4 GB (Settings > Resources > Memory).

### Viewing logs

```bash
# All services
docker compose --env-file .env.docker logs -f

# Single service
docker compose --env-file .env.docker logs -f api
docker compose --env-file .env.docker logs -f web
```

### Rebuilding a single service

```bash
docker compose --env-file .env.docker up --build api
```

## Architecture Notes

- **API image:** `mcr.microsoft.com/dotnet/aspnet:10.0` with `curl` for healthchecks. Runs as non-root (`$APP_UID`).
- **Web image:** `nginxinc/nginx-unprivileged:alpine` — non-root nginx on port 8080.
- **No authentication:** The Docker environment disables MSAL/Entra ID authentication. API endpoints requiring `[Authorize]` will return 401 unless auth is configured separately.
- **No Key Vault:** Azure Key Vault registration is skipped when `ASPNETCORE_ENVIRONMENT=Docker`.
- **Dummy App Insights:** A placeholder connection string prevents startup failures. Telemetry data is discarded.
- **Database not containerized:** Point the API at your existing SQL Server via the `CONNECTION_STRING` environment variable.
