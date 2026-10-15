#!/usr/bin/env bash
# Deploys a published version to one environment on the Pi.
# Usage: deploy.sh <dev|prod> <full-commit-sha>
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib.sh
. "$script_dir/lib.sh"

use_environment "${1:-}"
version="${2:-}"
[[ "$version" =~ ^[0-9a-f]{40}$ ]] || fail "version must be a full 40-character commit SHA (got '$version')"

# Install this version's compose file and scripts, so the stack and backups can be run by hand.
install -m 644 "$script_dir/compose.yaml" "$ENV_DIR/compose.yaml"
if [[ "$script_dir" != "$RISKYBUSINESS_ROOT/bin" ]]; then
  mkdir -p "$RISKYBUSINESS_ROOT/bin"
  install -m 755 "$script_dir"/{deploy,backup,restore}.sh "$RISKYBUSINESS_ROOT/bin/"
  install -m 644 "$script_dir"/{lib.sh,compose.yaml} "$RISKYBUSINESS_ROOT/bin/"
fi

previous="$(sed -n 's/^IMAGE_TAG=//p' "$ENV_DIR/version.env" 2>/dev/null || true)"

# Back up prod before every deployment, since the new version may migrate the database.
if [[ "$ENVIRONMENT" == prod && -n "$previous" ]]; then
  echo "Backing up prod before deploying..."
  "$script_dir/backup.sh" prod
fi

echo "Deploying $version to $ENVIRONMENT (previous: ${previous:-none})"
echo "IMAGE_TAG=$version" > "$ENV_DIR/version.env.new"
export VERSION_FILE="$ENV_DIR/version.env.new"

compose pull --quiet
compose up --detach --wait --remove-orphans

# Smoke test through the same entry point the tunnel uses: web -> api -> database.
port="$(web_port)"
if ! curl --fail --silent --show-error --retry 20 --retry-delay 3 --retry-all-errors \
    "http://127.0.0.1:$port/health/ready" > /dev/null; then
  compose logs --tail 100 api web >&2
  rm -f "$ENV_DIR/version.env.new"
  fail "smoke test failed for $version. To roll back, run the Deploy workflow with ${previous:-a previous version}."
fi

mv "$ENV_DIR/version.env.new" "$ENV_DIR/version.env"
echo "$(date -u +%Y-%m-%dT%H:%M:%SZ) $version" >> "$ENV_DIR/deployments.log"
docker image prune --force --filter "until=168h" > /dev/null

echo "Deployed $version to $ENVIRONMENT."
