#!/usr/bin/env bash
set -euo pipefail

TLA_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SPEC_ROOT="$TLA_ROOT/spec/formal"
CONFIG_DIR="$TLA_ROOT/test/formal/config"
JAR="${TLA2TOOLS_JAR:-$TLA_ROOT/tools/tla2tools.jar}"
WORK="${TMPDIR:-/tmp}/chatney-tla-sanity"
WORKFLOW="${1:-}"

case "$WORKFLOW" in
    ""|Users|Messages|Workspaces|Setup|Recovery) ;;
    *) echo "Expected Users, Messages, Workspaces, Setup, or Recovery." >&2; exit 2 ;;
esac

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

for CONFIG in "$CONFIG_DIR"/*Workflow.cfg; do
    CONFIG_NAME="$(basename "$CONFIG" .cfg)"

    MODULE_NAME="$CONFIG_NAME"

    if [[ "$CONFIG_NAME" == Messages*Workflow ]]; then
        MODULE_NAME="MessagesWorkflow"
    fi

    if [[ -n "$WORKFLOW" && "$MODULE_NAME" != "${WORKFLOW}Workflow" ]]; then
        continue
    fi

    echo "Checking $CONFIG_NAME"
    java -DTLA-Library="$SPEC_ROOT/workflows:$SPEC_ROOT/rules" \
        -cp "$JAR" tlc2.TLC -cleanup -metadir "$WORK/meta-$CONFIG_NAME" \
        -noGenerateSpecTE -config "$CONFIG" "$MODULE_NAME"
done
