#!/bin/bash
# Renders the game page images (#107) from card.html and icon.html with headless Chrome:
# the Unity Play thumbnail, the Open Graph image, the PWA icons, the favicon, the loading screen logo and the Player Settings thumbnail
# of the ChangPWA WebGL template.
# Usage (macOS, Google Chrome installed): Tools/PageArt/render.sh
set -euo pipefail

cd "$(dirname "$0")"
HERE="$(pwd)"
TEMPLATE="../../Assets/WebGLTemplates/ChangPWA/TemplateData"
THUMBNAIL="../../Assets/WebGLTemplates/ChangPWA/thumbnail.png" # template picture in Player Settings
CHROME="${CHROME:-/Applications/Google Chrome.app/Contents/MacOS/Google Chrome}"
TMP="$(mktemp -d)"

shot() { # page width height output
  "$CHROME" --headless --disable-gpu --hide-scrollbars --force-device-scale-factor=1 \
    --allow-file-access-from-files --default-background-color=00000000 \
    --virtual-time-budget=2000 --window-size="$2,$3" --screenshot="$4" "file://$HERE/$1" 2>/dev/null
}

shot card.html 1280 720 unity-play-thumbnail.png
shot card.html 1200 630 "$TEMPLATE/og-image.png"
shot icon.html 512 512 "$TEMPLATE/icons/icon-512.png"
shot icon.html 192 192 "$TEMPLATE/icons/icon-192.png"
shot "icon.html?maskable" 512 512 "$TEMPLATE/icons/icon-maskable-512.png"
shot "icon.html?maskable" 180 180 "$TEMPLATE/icons/apple-touch-icon.png"
shot icon.html 128 128 "$THUMBNAIL"
cp mascot.svg "$TEMPLATE/loading-mascot.svg" # loading screen logo
for s in 16 32 48; do shot icon.html $s $s "$TMP/favicon-$s.png"; done

# favicon.ico with PNG images inside (supported by all current browsers)
python3 - "$TMP" "$TEMPLATE/favicon.ico" <<'EOF'
import struct, sys
tmp, out = sys.argv[1], sys.argv[2]
images = [(s, open(f"{tmp}/favicon-{s}.png", "rb").read()) for s in (16, 32, 48)]
data = struct.pack("<HHH", 0, 1, len(images))
offset = 6 + 16 * len(images)
for size, png in images:
    data += struct.pack("<BBBBHHII", size, size, 0, 0, 1, 32, len(png), offset)
    offset += len(png)
data += b"".join(png for _, png in images)
open(out, "wb").write(data)
EOF

rm -rf "$TMP"
ls -l unity-play-thumbnail.png "$TEMPLATE/og-image.png" "$TEMPLATE/favicon.ico" "$TEMPLATE/icons/"*.png
