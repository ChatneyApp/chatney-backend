#!/usr/bin/env bash
set -euo pipefail

TLA_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SPEC_ROOT="$TLA_ROOT/spec/formal"
CONFIG_DIR="$TLA_ROOT/test/formal/config"
JAR="${TLA2TOOLS_JAR:-$TLA_ROOT/tools/tla2tools.jar}"
WORK="${TMPDIR:-/tmp}/chatney-tla-sanity"

if [[ ! -f "$JAR" ]]; then
    echo "TLC jar not found at $JAR; run test/formal/install-tlc.sh or set TLA2TOOLS_JAR." >&2
    exit 2
fi

mkdir -p "$WORK"
cd "$SPEC_ROOT/workflows"

while IFS= read -r module; do
    java -DTLA-Library="$SPEC_ROOT/workflows:$SPEC_ROOT/rules" \
        -cp "$JAR" tla2sany.SANY -error-codes "$module"
done < <(find "$SPEC_ROOT" -type f -name '*.tla' -print | sort)

for CONFIG in "$CONFIG_DIR"/Chatney*.cfg; do
    CONFIG_NAME="$(basename "$CONFIG" .cfg)"
    echo "Checking $CONFIG_NAME"
    java -DTLA-Library="$SPEC_ROOT/workflows:$SPEC_ROOT/rules" \
        -cp "$JAR" tlc2.TLC -cleanup -metadir "$WORK/meta-$CONFIG_NAME" \
        -noGenerateSpecTE -config "$CONFIG" ChatneyMain
done
