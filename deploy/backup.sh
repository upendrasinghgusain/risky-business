#!/usr/bin/env bash
# Writes a PostgreSQL dump for one environment and keeps the most recent 30.
# Usage: backup.sh <dev|prod>
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib.sh
. "$script_dir/lib.sh"

use_environment "${1:-}"

backup_dir="$RISKYBUSINESS_ROOT/backups/$ENVIRONMENT"
mkdir -p "$backup_dir"
file="$backup_dir/riskybusiness-$ENVIRONMENT-$(date -u +%Y%m%dT%H%M%SZ).dump"

# Written to a temporary name first, so a failed dump never looks like a valid backup.
compose exec -T db pg_dump --username riskybusiness --dbname riskybusiness --format custom > "$file.partial"
mv "$file.partial" "$file"

ls -1t "$backup_dir"/*.dump | tail -n +31 | xargs --no-run-if-empty rm --

echo "$file"
