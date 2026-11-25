#!/bin/bash
# One-command build + run of QuickproPos as a native macOS (MacCatalyst) app.
# Prereqs: .NET 8 SDK + `dotnet workload install maccatalyst maui-maccatalyst` + Xcode.
set -e
cd "$(dirname "$0")/../QuickproPos"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet build QuickproPos/QuickproPos/QuickproPos.csproj -t:Run \
  -f net8.0-maccatalyst -c Debug \
  -p:TargetFrameworks=net8.0-maccatalyst
