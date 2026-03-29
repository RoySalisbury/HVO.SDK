#!/bin/bash
set -e
set -o pipefail

echo "Running HVO.SDK post-create setup..."

# ─────────────────────────────────────────────────────────────────────
# Load shared devcontainer base script (common to all RoySalisbury repos)
# Provides: dc_bootstrap_env, dc_setup_dotnet, dc_install_cli,
#           dc_setup_docker, dc_setup_ssh, dc_create_contexts, dc_scan_hosts
# ─────────────────────────────────────────────────────────────────────
BASE_GIST="bceb71a9120e4d393b68308a03399ca5"

_load_base_script() {
	local token="${GH_TOKEN:-${GITHUB_TOKEN:-}}"
	if [ -z "$token" ] && command -v gh >/dev/null 2>&1; then
		token=$(gh auth token 2>/dev/null) || true
	fi
	if [ -z "$token" ] && command -v git >/dev/null 2>&1; then
		token=$(printf 'protocol=https\nhost=github.com\n' | GIT_TERMINAL_PROMPT=0 git credential fill 2>/dev/null | grep '^password=' | head -1 | cut -d= -f2-) || true
	fi
	if command -v gh >/dev/null 2>&1 && [ -n "$token" ]; then
		GH_TOKEN="$token" gh gist view "$BASE_GIST" --raw --filename devcontainer-base.sh 2>/dev/null && return 0
	fi
	curl -fsSL "https://gist.githubusercontent.com/RoySalisbury/${BASE_GIST}/raw/devcontainer-base.sh" 2>/dev/null && return 0
	return 1
}

BASE_SCRIPT=$(_load_base_script)
if [ -n "$BASE_SCRIPT" ]; then
	eval "$BASE_SCRIPT"
else
	echo "⚠  Could not load devcontainer-base.sh from gist. Continuing without shared setup."
fi

# ─────────────────────────────────────────────────────────────────────
# Resolve and export a GitHub token so base script functions can use it
# ─────────────────────────────────────────────────────────────────────
if [ -z "${GH_TOKEN:-}" ] && [ -z "${GITHUB_TOKEN:-}" ]; then
	_resolved_token=""
	if command -v gh >/dev/null 2>&1; then
		_resolved_token=$(gh auth token 2>/dev/null) || true
	fi
	if [ -z "$_resolved_token" ] && command -v git >/dev/null 2>&1; then
		_resolved_token=$(printf 'protocol=https\nhost=github.com\n' | GIT_TERMINAL_PROMPT=0 git credential fill 2>/dev/null | grep '^password=' | head -1 | cut -d= -f2-) || true
	fi
	if [ -n "$_resolved_token" ]; then
		export GH_TOKEN="$_resolved_token"
	fi
	unset _resolved_token
fi

# ─────────────────────────────────────────────────────────────────────
# Shared setup (.NET, CLI tools)
# ─────────────────────────────────────────────────────────────────────
HVOSDK_ENV_GIST="1f014918502877f0c37738fa733dad65"

if type dc_bootstrap_env >/dev/null 2>&1; then
	dc_bootstrap_env "$HVOSDK_ENV_GIST" "/workspaces/HVO.SDK"
	dc_setup_dotnet
	dc_install_cli
else
	echo "⚠  Base script not loaded — running inline fallback..."
	sudo chown -R vscode:vscode /home/vscode/.dotnet || true
	dotnet --info
	sudo apt-get update -y && sudo apt-get install -y jq ripgrep || true
fi

# ─────────────────────────────────────────────────────────────────────
# HVO.SDK-specific setup
# ─────────────────────────────────────────────────────────────────────

# Install global .NET tools
dotnet tool install --global dotnet-reportgenerator-globaltool 2>/dev/null || true

# Restore NuGet packages
dotnet restore || echo "Warning: dotnet restore failed, continuing..."

echo
echo "Post-create setup completed successfully!"
