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

## Deploy to Kubernetes (k3s / Docker Desktop / k3d)

Manifests live in `k8s/` (Kustomize). Images use `imagePullPolicy: Never`.

### Windows (Docker Desktop) — recommended

1. **Docker Desktop → Settings → Kubernetes → Enable Kubernetes** (wait until green).
2. Fix image pulls if needed: `docker pull mcr.microsoft.com/dotnet/sdk:10.0`  
   If that fails with `lookup mcr.microsoft.com: no such host`, Docker DNS is broken (VPN, corporate DNS, or Docker Desktop network). Restart Docker Desktop or set a public DNS (e.g. 8.8.8.8) under Settings → Resources → Network.
3. Deploy:

```powershell
./scripts/deploy-k3s.ps1 -Runtime docker-desktop
```

### k3d

```powershell
k3d cluster create darkwing
./scripts/deploy-k3s.ps1 -Runtime k3d -K3dCluster darkwing
```

### Native k3s (Linux / WSL only)

```powershell
./scripts/deploy-k3s.ps1 -Runtime k3s
```

### Access

| Path | How |
|------|-----|
| Port-forward | `kubectl -n darkwing port-forward svc/darkwing-api 8080:80` → http://localhost:8080/weatherforecast |
| Ingress (Traefik) | http://darkwing.localhost/weatherforecast |
| LoadBalancer | `kubectl -n darkwing get svc darkwing-api` |

```powershell
kubectl -n darkwing get pods,svc,ingress
kubectl -n darkwing logs -f deploy/darkwing-worker
```
