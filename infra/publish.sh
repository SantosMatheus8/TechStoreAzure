#!/bin/bash
set -e

echo "Publicando TechStore API..."

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/../src/TechStore.Api"
OUTPUT_DIR="$SCRIPT_DIR/../artifacts"

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

dotnet publish "$PROJECT_DIR" \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o "$OUTPUT_DIR/publish"

echo "Compactando artefatos..."
tar -czf "$OUTPUT_DIR/techstore-api.tar.gz" -C "$OUTPUT_DIR/publish" .

echo ""
echo "Publicação concluída!"
echo "Pacote: $OUTPUT_DIR/techstore-api.tar.gz"
echo "Tamanho: $(du -h "$OUTPUT_DIR/techstore-api.tar.gz" | cut -f1)"
