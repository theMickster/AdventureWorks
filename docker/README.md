# Containerized API and web workflow

This Compose stack runs only the AdventureWorks API and Angular app. Aspire is the canonical unified local-development experience and owns Functions, Azurite, the Service Bus emulator, and the saga test harness.

## Start

Prerequisites are Docker Compose v2, ports 5000 and 4200, and an externally managed AdventureWorks SQL Server reachable from containers.

```bash
cp docker/.env.example docker/.env
docker compose --project-directory docker --env-file docker/.env -f docker/compose.yml up --build
```

The web app is at `http://localhost:4200`, the API at `http://localhost:5000`, and API health at `http://localhost:5000/health`.

```bash
docker compose --project-directory docker --env-file docker/.env -f docker/compose.yml down
```

Use `host.docker.internal` in `CONNECTION_STRING` to reach SQL Server on the host. If a port is occupied, change its host-side mapping in `compose.yml`.

For logs or a targeted rebuild:

```bash
docker compose --project-directory docker --env-file docker/.env -f docker/compose.yml logs -f
docker compose --project-directory docker --env-file docker/.env -f docker/compose.yml up --build api
```

The DbReset backup and permissions workflow is documented in [`../tools/console-apps/AdventureWorks.DbReset.Console/README.md`](../tools/console-apps/AdventureWorks.DbReset.Console/README.md).
