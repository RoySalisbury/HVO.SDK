#!/usr/bin/env bash
set -euo pipefail

echo "=== HVO.SDK post-create ==="

# Install global .NET tools
dotnet tool install --global dotnet-reportgenerator-globaltool 2>/dev/null || true

# Restore NuGet packages
dotnet restore

echo "=== post-create complete ==="
