#!/bin/bash
# Makes the tiny web fonts of the ChangPWA start/loading screen: only the glyphs the page shows.
# Charmonman Bold for the logo "ช้าง", Prompt SemiBold for "Play", digits and "%". Both fonts are OFL (no reserved names).
# Usage: pip install fonttools brotli; Tools/PageArt/fonts.sh
# If the page shows other text in these fonts, add its characters below and run again.
set -euo pipefail

cd "$(dirname "$0")"
FONTS="../../Assets/Project/Fonts"
TEMPLATE="../../Assets/WebGLTemplates/ChangPWA/TemplateData"

pyftsubset "$FONTS/Charmonman/Charmonman-Bold.ttf" --text="ช้าง" --layout-features='*' --flavor=woff2 \
  --output-file="$TEMPLATE/logo-charmonman.woff2"
pyftsubset "$FONTS/Prompt/Static/Prompt-SemiBold.ttf" --text="Play0123456789% " --layout-features='*' --flavor=woff2 \
  --output-file="$TEMPLATE/prompt-semibold.woff2"

ls -l "$TEMPLATE/"*.woff2
