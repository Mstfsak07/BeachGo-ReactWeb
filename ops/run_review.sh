#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PYTHON_BIN="${PYTHON_BIN:-python}"

if [[ $# -lt 1 ]]; then
  echo "Usage: ./ops/run_review.sh <assessment-file> [--execute] [extra args...]"
  exit 1
fi

MODE="dry-run"
ARGS=()

for arg in "$@"; do
  if [[ "$arg" == "--execute" ]]; then
    MODE="execute"
  else
    ARGS+=("$arg")
  fi
done

exec "$PYTHON_BIN" "$ROOT_DIR/ops/review_orchestrator.py" "${ARGS[@]}" --mode "$MODE"
