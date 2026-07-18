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

## Auto-deploy to VDS (push to `main`)

On every push to `main`, GitHub Actions SSHs into the VDS, pulls the repo, builds images, imports them into k3s, and applies `k8s/`.

### One-time VDS setup

1. Install Docker and [k3s](https://docs.k3s.io/quick-start) on the Linux VDS.
2. Give the deploy user kubectl access and passwordless sudo for image import:

```bash
mkdir -p ~/.kube
sudo cp /etc/rancher/k3s/k3s.yaml ~/.kube/config
sudo chown "$USER":"$USER" ~/.kube/config
# optional: allow only k3s ctr without a password prompt
echo "$USER ALL=(root) NOPASSWD: /usr/local/bin/k3s" | sudo tee /etc/sudoers.d/darkwing-k3s
```

3. Clone the repo (use a read-only [deploy key](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/managing-deploy-keys) if the repo is private):

```bash
sudo mkdir -p /opt/darkwing
sudo chown "$USER":"$USER" /opt/darkwing
git clone git@github.com:petelinmn/darkwing.git /opt/darkwing
```

4. Confirm a manual deploy works:

```bash
cd /opt/darkwing
chmod +x scripts/deploy-vds.sh
./scripts/deploy-vds.sh
```

5. Create a dedicated SSH key for Actions, put the **public** key in the VDS user's `~/.ssh/authorized_keys`, and store the **private** key in GitHub as `VDS_SSH_KEY`.

### GitHub secrets

Repository → **Settings → Secrets and variables → Actions**:

| Secret | Example | Required |
|--------|---------|----------|
| `VDS_HOST` | `203.0.113.10` | yes |
| `VDS_USER` | `deploy` | yes |
| `VDS_SSH_KEY` | private key PEM (`-----BEGIN ...`) | yes |
| `VDS_PORT` | `22` | yes (use `22` if default) |
| `VDS_APP_DIR` | `/opt/darkwing` | no (defaults to `/opt/darkwing` if empty) |

After secrets are set, push to `main` (or run **Actions → Deploy to VDS → Run workflow**). The workflow file is in the repo, so commit and push these changes once to enable it.