#!/usr/bin/env bash
# Replaces an environment's database with a backup made by backup.sh.
# Usage: restore.sh <dev|prod> <dump-file> --yes
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib.sh
. "$script_dir/lib.sh"

use_environment "${1:-}"
dump="${2:-}"
[[ -f "$dump" ]] || fail "dump file not found: '$dump'"
[[ "${3:-}" == "--yes" ]] || fail "this replaces all $ENVIRONMENT data with $dump; re-run with --yes to confirm"

echo "Stopping the API so nothing writes during the restore..."
compose stop api

compose exec -T db pg_restore --username riskybusiness --dbname riskybusiness \
  --clean --if-exists --no-owner --exit-on-error < "$dump"

compose start api
echo "Restored $ENVIRONMENT from $dump."
