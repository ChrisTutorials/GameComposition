#!/usr/bin/env bash
set -euo pipefail

ARTIFACTS_DIR="${1:-artifacts}"

if [ ! -d "$ARTIFACTS_DIR" ]; then
  echo "[verify-pack] artifacts dir not found: $ARTIFACTS_DIR" >&2
  exit 1
fi

NUPKG=""
for f in "$ARTIFACTS_DIR"/*.nupkg; do
  if [ -f "$f" ]; then
    NUPKG="$f"
    break
  fi
done

if [ -z "$NUPKG" ]; then
  echo "[verify-pack] no .nupkg found in: $ARTIFACTS_DIR" >&2
  ls -la "$ARTIFACTS_DIR" || true
  exit 1
fi

echo "[verify-pack] verifying: $NUPKG"

tmp_list="$(mktemp)"
trap 'rm -f "$tmp_list"' EXIT

if command -v unzip >/dev/null 2>&1; then
  unzip -l "$NUPKG" > "$tmp_list"
elif command -v bsdtar >/dev/null 2>&1; then
  bsdtar -tf "$NUPKG" > "$tmp_list"
else
  echo "[verify-pack] need unzip or bsdtar to inspect nupkg" >&2
  exit 1
fi

required_paths=(
  "lib/net8.0/GameComposition.Core.dll"
  "lib/net8.0/GameComposition.Core.xml"
)

missing=0
for p in "${required_paths[@]}"; do
  if ! grep -Fq "$p" "$tmp_list"; then
    echo "[verify-pack] MISSING: $p" >&2
    missing=1
  fi
done

if [ "$missing" -ne 0 ]; then
  echo "[verify-pack] package verification failed" >&2
  exit 1
fi

echo "[verify-pack] OK"
