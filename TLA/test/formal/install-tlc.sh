#!/usr/bin/env bash
set -euo pipefail

TLA_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
DEST="$TLA_ROOT/tools/tla2tools.jar"
URL="https://github.com/tlaplus/tlaplus/releases/download/v1.8.0/tla2tools.jar"
EXPECTED_SHA256="e1f11b75c4b5bc4478ba0ac5cb6ae28e40a01b0016bc33e0aa97da22dfdbcd7f"

mkdir -p "$(dirname "$DEST")"
curl --fail --location --silent --show-error "$URL" --output "$DEST"
ACTUAL_SHA256="$(shasum -a 256 "$DEST" | awk '{print $1}')"

if [[ "$ACTUAL_SHA256" != "$EXPECTED_SHA256" ]]; then
    rm -f "$DEST"
    echo "TLC checksum mismatch: expected $EXPECTED_SHA256, got $ACTUAL_SHA256" >&2
    exit 1
fi

echo "Installed verified TLA+ Tools 1.8.0 at $DEST"
