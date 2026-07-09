#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SERVICE_LABEL="com.stockpile.api.agent-local"
BUILD_DIR="$ROOT_DIR/Stockpile.Api/bin/Release/net10.0"
API_DLL="$BUILD_DIR/Stockpile.Api.dll"
LOG_DIR="$ROOT_DIR/.local/logs"
GENERATED_DIR="$ROOT_DIR/.local/generated"
PLIST_PATH="$GENERATED_DIR/$SERVICE_LABEL.plist"
LAUNCH_AGENTS_DIR="$HOME/Library/LaunchAgents"
INSTALLED_PLIST="$LAUNCH_AGENTS_DIR/$SERVICE_LABEL.plist"
DOTNET_BIN="${DOTNET_BIN:-/usr/local/share/dotnet/dotnet}"

if [[ -f "$ROOT_DIR/.env.agent-local" ]]; then
  set -a
  # shellcheck source=/dev/null
  source "$ROOT_DIR/.env.agent-local"
  set +a
fi

xml_escape() {
  local value="${1:-}"
  value="${value//&/&amp;}"
  value="${value//</&lt;}"
  value="${value//>/&gt;}"
  value="${value//\"/&quot;}"
  value="${value//\'/&apos;}"
  printf '%s' "$value"
}

AGENT_API_KEY_SHA_XML="$(xml_escape "${InventoryAgent__ApiKeySha256:-}")"
AGENT_USER_ID_XML="$(xml_escape "${InventoryAgent__UserId:-}")"
AGENT_USERNAME_XML="$(xml_escape "${InventoryAgent__Username:-}")"

if [[ -z "${InventoryAgent__ApiKeySha256:-}" || -z "${InventoryAgent__UserId:-}" || -z "${InventoryAgent__Username:-}" ]]; then
  echo "Warning: InventoryAgent credentials are incomplete. Agent endpoints will reject requests until .env.agent-local is configured." >&2
fi

mkdir -p "$LOG_DIR" "$GENERATED_DIR" "$LAUNCH_AGENTS_DIR"

docker compose -f "$ROOT_DIR/compose.yaml" up -d InventoryDatabase

"$DOTNET_BIN" build "$ROOT_DIR/Stockpile.Api/Stockpile.Api.csproj" \
  -c Release

(
  cd "$ROOT_DIR/Stockpile.Api"
  ASPNETCORE_ENVIRONMENT="AgentLocal" \
  ASPNETCORE_URLS="http://127.0.0.1:8080" \
    "$DOTNET_BIN" "$API_DLL" --initialize-database
)

cat > "$PLIST_PATH" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>$SERVICE_LABEL</string>
  <key>ProgramArguments</key>
  <array>
    <string>$DOTNET_BIN</string>
    <string>$API_DLL</string>
  </array>
  <key>WorkingDirectory</key>
  <string>$ROOT_DIR/Stockpile.Api</string>
  <key>EnvironmentVariables</key>
  <dict>
    <key>ASPNETCORE_ENVIRONMENT</key>
    <string>AgentLocal</string>
    <key>ASPNETCORE_URLS</key>
    <string>http://127.0.0.1:8080</string>
    <key>InventoryAgent__ApiKeySha256</key>
    <string>$AGENT_API_KEY_SHA_XML</string>
    <key>InventoryAgent__UserId</key>
    <string>$AGENT_USER_ID_XML</string>
    <key>InventoryAgent__Username</key>
    <string>$AGENT_USERNAME_XML</string>
  </dict>
  <key>RunAtLoad</key>
  <true/>
  <key>KeepAlive</key>
  <true/>
  <key>StandardOutPath</key>
  <string>$LOG_DIR/stockpile-api.out.log</string>
  <key>StandardErrorPath</key>
  <string>$LOG_DIR/stockpile-api.err.log</string>
</dict>
</plist>
PLIST

cp "$PLIST_PATH" "$INSTALLED_PLIST"
launchctl bootout "gui/$(id -u)" "$INSTALLED_PLIST" 2>/dev/null || true
launchctl bootstrap "gui/$(id -u)" "$INSTALLED_PLIST"
launchctl kickstart -k "gui/$(id -u)/$SERVICE_LABEL"

for _ in {1..30}; do
  if curl -fsS "http://127.0.0.1:8080/healthz" >/dev/null 2>&1; then
    echo "Stockpile API is healthy at http://127.0.0.1:8080"
    echo "OpenAPI document is available at http://127.0.0.1:8080/swagger/v1/swagger.json"
    exit 0
  fi

  sleep 1
done

echo "Stockpile API did not become healthy. Check $LOG_DIR/stockpile-api.err.log" >&2
exit 1
