#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs frontend CI checks: lint, tests, build
.DESCRIPTION
    Comprehensive frontend quality checks for local development
#>

param(
    [switch]$Fix,
    [switch]$SkipTests,
    [switch]$CoverageReport
)

$ErrorActionPreference = "Stop"
$ClientDir = "RedCrossManager.Client"

Write-Host "🔍 Frontend CI Checks" -ForegroundColor Cyan
Write-Host "Working directory: $ClientDir" -ForegroundColor DarkGray

# Check for Node.js
Write-Host "`n✓ Checking Node.js..." -ForegroundColor Green
$nodeVersion = node --version
Write-Host "  Using Node $nodeVersion"

Push-Location $ClientDir

try {
    # Install dependencies
    Write-Host "`n📦 Installing dependencies..." -ForegroundColor Green
    npm ci --prefer-offline --no-audit
    if ($LASTEXITCODE -ne 0) { exit 1 }

    # ESLint
    Write-Host "`n🔎 Running ESLint..." -ForegroundColor Green
    if ($Fix) {
        npm run lint -- --fix 2>/dev/null
    } else {
        npm run lint 2>/dev/null
    }
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "⚠️  ESLint warnings/errors found (non-blocking)" -ForegroundColor Yellow
    } else {
        Write-Host "✅ ESLint passed" -ForegroundColor Green
    }

    # Prettier
    Write-Host "`n✨ Checking formatting with Prettier..." -ForegroundColor Green
    if ($Fix) {
        npx prettier --write "src/**/*.{ts,tsx,js,jsx,html,scss,css,json}" 2>/dev/null
        Write-Host "✅ Code formatted" -ForegroundColor Green
    } else {
        npx prettier --check "src/**/*.{ts,tsx,js,jsx,html,scss,css,json}" 2>/dev/null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "⚠️  Formatting issues found (run with -Fix to auto-fix)" -ForegroundColor Yellow
        } else {
            Write-Host "✅ Formatting check passed" -ForegroundColor Green
        }
    }

    # Tests
    if (-not $SkipTests) {
        Write-Host "`n🧪 Running tests..." -ForegroundColor Green
        if ($CoverageReport) {
            npm run test -- --code-coverage --watch=false --browsers=ChromeHeadless 2>&1 | Select-String -Pattern "(✓|✗|FAILED|passed)" -ErrorAction SilentlyContinue
        } else {
            npm run test -- --watch=false --browsers=ChromeHeadless 2>&1 | Select-String -Pattern "(✓|✗|FAILED|passed)" -ErrorAction SilentlyContinue
        }
        
        if ($LASTEXITCODE -ne 0) { 
            Write-Host "❌ Tests failed" -ForegroundColor Red
            exit 1 
        }
        Write-Host "✅ Tests passed" -ForegroundColor Green
    }

    # Build
    Write-Host "`n🏗️ Building for production..." -ForegroundColor Green
    npm run build
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "❌ Build failed" -ForegroundColor Red
        exit 1 
    }
    Write-Host "✅ Build successful" -ForegroundColor Green

    Write-Host "`n✨ Frontend CI checks passed!" -ForegroundColor Green
    exit 0
}
finally {
    Pop-Location
}
