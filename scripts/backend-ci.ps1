#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs backend CI checks: format, lint (StyleCop), build, and tests
.DESCRIPTION
    Comprehensive backend quality checks for local development
#>

param(
    [switch]$Fix,
    [switch]$SkipTests,
    [switch]$CoverageReport
)

$ErrorActionPreference = "Stop"
$BuildConfiguration = "Release"

Write-Host "🔍 Backend CI Checks" -ForegroundColor Cyan

# Check for .NET installation
Write-Host "✓ Checking .NET SDK..." -ForegroundColor Green
$dotnetVersion = dotnet --version
Write-Host "  Using .NET $dotnetVersion"

# Restore
Write-Host "`n📦 Restoring dependencies..." -ForegroundColor Green
dotnet restore
if ($LASTEXITCODE -ne 0) { exit 1 }

# Format (optional fix)
if ($Fix) {
    Write-Host "`n🎨 Formatting code with dotnet-format..." -ForegroundColor Green
    dotnet format
}

# Build
Write-Host "`n🏗️ Building solution..." -ForegroundColor Green
dotnet build --no-restore --configuration $BuildConfiguration /p:EnforceCodeStyleInBuild=true
if ($LASTEXITCODE -ne 0) { 
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1 
}
Write-Host "✅ Build successful" -ForegroundColor Green

# Tests
if (-not $SkipTests) {
    Write-Host "`n🧪 Running tests..." -ForegroundColor Green
    if ($CoverageReport) {
        dotnet test RedCrossManager.Server.Tests/RedCrossManager.Server.Tests.csproj `
            --no-build `
            --configuration $BuildConfiguration `
            --logger "console;verbosity=normal" `
            --collect:"XPlat Code Coverage" `
            -- RunConfiguration.DisableAppDomain=true
    } else {
        dotnet test RedCrossManager.Server.Tests/RedCrossManager.Server.Tests.csproj `
            --no-build `
            --configuration $BuildConfiguration `
            --logger "console;verbosity=normal"
    }
    
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "❌ Tests failed" -ForegroundColor Red
        exit 1 
    }
    Write-Host "✅ Tests passed" -ForegroundColor Green
}

Write-Host "`n✨ Backend CI checks passed!" -ForegroundColor Green
exit 0
