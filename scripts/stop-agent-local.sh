#!/usr/bin/env bash
set -euo pipefail

SERVICE_LABEL="com.stockpile.api.agent-local"
INSTALLED_PLIST="$HOME/Library/LaunchAgents/$SERVICE_LABEL.plist"

launchctl bootout "gui/$(id -u)" "$INSTALLED_PLIST" 2>/dev/null || true
