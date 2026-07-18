#!/usr/bin/env bash
# Build images on the VDS, import into local k3s, apply manifests, restart pods.
# Prerequisites on the server: docker, k3s (provides kubectl), git clone of this repo.
#
# Usage (from repo root):
#   ./scripts/deploy-vds.sh
#   IMAGE_TAG=abc1234 ./scripts/deploy-vds.sh

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

API_IMAGE="${API_IMAGE:-darkwing-api}"
WORKER_IMAGE="${WORKER_IMAGE:-darkwing-worker}"
IMAGE_TAG="${IMAGE_TAG:-local}"
API_REF="${API_IMAGE}:${IMAGE_TAG}"
WORKER_REF="${WORKER_IMAGE}:${IMAGE_TAG}"

need() {
  command -v "$1" >/dev/null 2>&1 || {
    echo "error: '$1' not found on PATH" >&2
    exit 1
  }
}

need docker
need kubectl

if ! kubectl cluster-info >/dev/null 2>&1; then
  echo "error: kubectl cannot reach the cluster (is k3s running?)" >&2
  exit 1
fi

echo "Building ${API_REF} ..."
docker build -t "${API_REF}" -t "${API_IMAGE}:local" -f src/Darkwing.Api/Dockerfile .

echo "Building ${WORKER_REF} ..."
docker build -t "${WORKER_REF}" -t "${WORKER_IMAGE}:local" -f src/Darkwing.Worker/Dockerfile .

echo "Importing images into k3s ..."
docker save "${API_IMAGE}:local" | sudo k3s ctr images import -
docker save "${WORKER_IMAGE}:local" | sudo k3s ctr images import -

echo "Applying manifests ..."
kubectl apply -k k8s

echo "Restarting deployments so pods pick up rebuilt :local images ..."
kubectl -n darkwing rollout restart deploy/darkwing-api deploy/darkwing-worker
kubectl -n darkwing rollout status deploy/darkwing-api --timeout=180s
kubectl -n darkwing rollout status deploy/darkwing-worker --timeout=180s

echo ""
echo "Deployed. Status:"
kubectl -n darkwing get pods,svc,ingress
echo ""
echo "API via LoadBalancer EXTERNAL-IP / NodePort, or:"
echo "  kubectl -n darkwing port-forward svc/darkwing-api 8080:80"
