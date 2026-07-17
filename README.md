# Darkwing

.NET 10 Web API + Worker with Docker Compose support for **Visual Studio** and CLI.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop
- Visual Studio 2022 17.14+ / Visual Studio 2026 with **Container Tools** workload

## Visual Studio (Docker Compose debug)

1. Open `Darkwing.slnx`.
2. In Solution Explorer, right-click **docker-compose** → **Set as Startup Project**.
3. Confirm the toolbar debug target shows **Docker Compose**.
4. Press **F5**.

Visual Studio builds both containers, starts them, and attaches the debugger to `darkwing.api` and `darkwing.worker`.

If you still do not see Docker Compose:

- Install workload: **ASP.NET and web development** + **Container development tools**
- Ensure Docker Desktop is running (Linux containers)
- Reload the solution after `docker-compose.dcproj` was added

## Run locally (without Docker)

```powershell
dotnet run --project src/Darkwing.Api
dotnet run --project src/Darkwing.Worker
```

API: http://localhost:5161/weatherforecast

## Run with Docker Compose (CLI)

```powershell
docker compose up --build
```

- API: http://localhost:8080/weatherforecast
- Worker logs: `docker compose logs -f darkwing.worker`
