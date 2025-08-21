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
