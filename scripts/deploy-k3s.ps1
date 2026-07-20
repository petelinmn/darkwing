# Build images, load them into the cluster (if needed), then apply k8s manifests.
# Prerequisites: Docker Desktop, kubectl pointed at a running cluster.
#
# Windows (recommended): enable Kubernetes in Docker Desktop, then:
#   ./scripts/deploy-k3s.ps1 -Runtime docker-desktop
#
# k3d:
#   ./scripts/deploy-k3s.ps1 -Runtime k3d -K3dCluster darkwing
#
# Native k3s (Linux / WSL):
#   ./scripts/deploy-k3s.ps1 -Runtime k3s

param(
    [ValidateSet("docker-desktop", "k3d", "k3s")]
    [string]$Runtime = $(if ($IsWindows -or $env:OS -match "Windows") { "docker-desktop" } else { "k3s" }),

    [string]$ApiImage = "darkwing-api:local",
    [string]$WorkerImage = "darkwing-worker:local",
    [string]$PricePickerImage = "darkwing-pricepicker:local",

    # Used only when -Runtime k3d
    [string]$K3dCluster = "darkwing"
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

function Assert-LastExitCode([string]$Step) {
    if ($LASTEXITCODE -ne 0) {
        throw "$Step failed (exit code $LASTEXITCODE)."
    }
}

function Assert-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "'$Name' was not found on PATH. Install it and retry."
    }
}

Assert-Command docker
Assert-Command kubectl

Write-Host "Checking cluster connectivity ..."
$kubeConfig = Join-Path $env:USERPROFILE ".kube\config"
if (-not (Test-Path $kubeConfig)) {
    throw @"
No kubeconfig found at $kubeConfig (kubectl falls back to http://localhost:8080 and fails).

On Windows, create a cluster first:

  Option A — Docker Desktop
    1. Docker Desktop → Settings → Kubernetes → Enable Kubernetes
    2. Apply & Restart, wait until Kubernetes shows green
    3. Verify:  kubectl config get-contexts
       (you should see 'docker-desktop')
    4. Re-run:  .\deploy-k3s.ps1 -Runtime docker-desktop

  Option B — k3d
    winget install k3d.k3d
    k3d cluster create darkwing
    .\deploy-k3s.ps1 -Runtime k3d -K3dCluster darkwing
"@
}

$ErrorActionPreference = "Continue"
kubectl cluster-info 2>&1 | Out-Null
$clusterOk = ($LASTEXITCODE -eq 0)
$ErrorActionPreference = "Stop"
if (-not $clusterOk) {
    throw @"
kubectl cannot reach the API server.

  kubectl config get-contexts
  kubectl config use-context docker-desktop   # or your k3d context

If Docker Desktop Kubernetes is enabled but still failing, open Docker Desktop and wait until Kubernetes is Running.
"@
}

Write-Host "Building $ApiImage ..."
docker build -t $ApiImage -f "$Root/src/Darkwing.Api/Dockerfile" $Root
Assert-LastExitCode "docker build ($ApiImage). If DNS fails for mcr.microsoft.com, fix Docker Desktop DNS/VPN, then retry."

Write-Host "Building $WorkerImage ..."
docker build -t $WorkerImage -f "$Root/src/Darkwing.Worker/Dockerfile" $Root
Assert-LastExitCode "docker build ($WorkerImage)"

Write-Host "Building $PricePickerImage ..."
docker build -t $PricePickerImage -f "$Root/src/PricePicker/Dockerfile" $Root
Assert-LastExitCode "docker build ($PricePickerImage)"

switch ($Runtime) {
    "docker-desktop" {
        # Docker Desktop kubeadm uses containerd; docker-built images must be imported into the node.
        # Prefer /root over /tmp: /tmp is a small tmpfs and docker cp can fail there.
        $node = "desktop-control-plane"
        $tmp = Join-Path $env:TEMP "darkwing-images"
        New-Item -ItemType Directory -Force -Path $tmp | Out-Null

        Write-Host "Importing images into Docker Desktop node '$node' ..."
        foreach ($image in @($ApiImage, $WorkerImage, $PricePickerImage)) {
            $safe = ($image -replace "[:/]", "-")
            $tar = Join-Path $tmp "$safe.tar"
            docker save $image -o $tar
            Assert-LastExitCode "docker save ($image)"
            docker cp $tar "${node}:/root/$safe.tar"
            Assert-LastExitCode "docker cp ($image)"
            docker exec $node ctr -n k8s.io images import "/root/$safe.tar"
            Assert-LastExitCode "ctr import ($image)"
        }
    }
    "k3d" {
        Assert-Command k3d
        Write-Host "Importing images into k3d cluster '$K3dCluster' ..."
        k3d image import $ApiImage $WorkerImage $PricePickerImage -c $K3dCluster
        Assert-LastExitCode "k3d image import"
    }
    "k3s" {
        if ($IsWindows -or $env:OS -match "Windows") {
            throw "Native k3s import requires Linux/WSL with 'k3s ctr'. On Windows use -Runtime docker-desktop or k3d."
        }
        Write-Host "Importing images into k3s via ctr ..."
        docker save $ApiImage | sudo k3s ctr images import -
        Assert-LastExitCode "k3s ctr import ($ApiImage)"
        docker save $WorkerImage | sudo k3s ctr images import -
        Assert-LastExitCode "k3s ctr import ($WorkerImage)"
        docker save $PricePickerImage | sudo k3s ctr images import -
        Assert-LastExitCode "k3s ctr import ($PricePickerImage)"
    }
}

Write-Host "Applying manifests ..."
kubectl apply -k "$Root/k8s"
Assert-LastExitCode "kubectl apply"

Write-Host ""
Write-Host "Done. Useful commands:"
Write-Host "  kubectl -n darkwing get pods,svc,ingress"
Write-Host "  kubectl -n darkwing logs -f deploy/darkwing-api"
Write-Host "  kubectl -n darkwing logs -f deploy/darkwing-worker"
Write-Host "  kubectl -n darkwing logs -f deploy/pricepicker"
Write-Host "  kubectl -n darkwing port-forward svc/darkwing-api 8080:80"
Write-Host "  curl http://localhost:8080/weatherforecast"
