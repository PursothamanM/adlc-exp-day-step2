#!/bin/sh
set -eu

HTML_DIR="/usr/share/nginx/html"
INDEX_FILE="$HTML_DIR/index.html"

if [ -f "$INDEX_FILE" ]; then
  # Escape for sed replacement: escape / and &
  VALUE_ESCAPED=$(printf '%s' "${VITE_API_URL:-}" | sed -e 's/[\/&]/\\&/g')
  sed -i "s|__VITE_API_URL__|$VALUE_ESCAPED|g" "$INDEX_FILE"
fi

exec "$@"
