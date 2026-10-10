#!/bin/zsh
# Builds the Chang Voice app without an Xcode project (like the SubDub app).
#   ./build.sh          → build/Chang Voice.app
#   ./build.sh open     → build and open it
# Needs Xcode: the app voices the words through its Swift interpreter (Siri voices), and swiftc builds the app.
set -euo pipefail
cd "${0:A:h}"

export DEVELOPER_DIR="${DEVELOPER_DIR:-/Applications/Xcode.app/Contents/Developer}"
SDK="$(xcrun --sdk macosx --show-sdk-path)"
ARCH="$(uname -m)"
APP="build/Chang Voice.app"

rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"

xcrun swiftc -sdk "$SDK" -target "$ARCH-apple-macos14.0" -swift-version 5 -O -parse-as-library \
  Sources/*.swift -o "$APP/Contents/MacOS/ChangVoice"
cp Resources/siri-tts.swift "$APP/Contents/Resources/"

cat > "$APP/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
  <key>CFBundleName</key><string>Chang Voice</string>
  <key>CFBundleDisplayName</key><string>Chang Voice</string>
  <key>CFBundleIdentifier</key><string>com.chang.voice</string>
  <key>CFBundleExecutable</key><string>ChangVoice</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleShortVersionString</key><string>1.0</string>
  <key>CFBundleVersion</key><string>1</string>
  <key>LSMinimumSystemVersion</key><string>14.0</string>
  <key>NSHighResolutionCapable</key><true/>
</dict></plist>
PLIST

codesign -f -s - "$APP"
echo "Built $APP"
[[ "${1:-}" == "open" ]] && open "$APP"
true
