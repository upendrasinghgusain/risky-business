# Shared helpers for the deploy scripts. Sourced, not executed.

RISKYBUSINESS_ROOT="${RISKYBUSINESS_ROOT:-/opt/riskybusiness}"

fail() {
  echo "error: $*" >&2
  exit 1
}

# Validates the environment name and sets ENV_DIR.
use_environment() {
  [[ "${1:-}" =~ ^(dev|prod)$ ]] || fail "environment must be 'dev' or 'prod' (got '${1:-}')"
  ENVIRONMENT="$1"
  ENV_DIR="$RISKYBUSINESS_ROOT/$ENVIRONMENT"
  [[ -f "$ENV_DIR/.env" ]] || fail "missing $ENV_DIR/.env (see docs/runbooks/pi-setup.md)"
}

# docker compose for the selected environment. Each environment is its own Compose project,
# so containers, networks and the database volume are fully separate.
compose() {
  local version_file="${VERSION_FILE:-$ENV_DIR/version.env}"
  docker compose \
    --project-name "riskybusiness-$ENVIRONMENT" \
    --project-directory "$ENV_DIR" \
    --file "$ENV_DIR/compose.yaml" \
    --env-file "$ENV_DIR/.env" \
    --env-file "$version_file" \
    "$@"
}

web_port() {
  # shellcheck disable=SC1091
  (set -a && . "$ENV_DIR/.env" && echo "$WEB_PORT")
}
