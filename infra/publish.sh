#!/bin/bash
set -e

echo "Publicando TechStore API..."

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/../src/TechStore.Api"
OUTPUT_DIR="$SCRIPT_DIR/../artifacts"

rm -rf "$OUTPUT_DIR"

dotnet publish "$PROJECT_DIR" \
    -c Release \
    -o "$OUTPUT_DIR" \
    --self-contained false

echo "Publicação concluída em: $OUTPUT_DIR"
echo "Arquivos gerados:"
ls -lh "$OUTPUT_DIR"
