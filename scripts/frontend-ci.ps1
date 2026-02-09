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

Write-Host "Frontend CI Checks" -ForegroundColor Cyan
Write-Host "Working directory: $ClientDir" -ForegroundColor DarkGray

# Check for Node.js
Write-Host "`nChecking Node.js..." -ForegroundColor Green
$nodeVersion = node --version
Write-Host "  Using Node $nodeVersion"

Push-Location $ClientDir

try {
    # Install dependencies
    Write-Host "`nInstalling dependencies..." -ForegroundColor Green
    npm ci --prefer-offline --no-audit
    if ($LASTEXITCODE -ne 0) { exit 1 }

    # ESLint
    Write-Host "`nRunning ESLint..." -ForegroundColor Green
    if ($Fix) {
        npm run lint -- --fix 2>$null
    } else {
        npm run lint 2>$null
    }
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "ESLint warnings/errors found (non-blocking)" -ForegroundColor Yellow
    } else {
        Write-Host "ESLint passed" -ForegroundColor Green
    }

    # Prettier
    Write-Host "`nChecking formatting with Prettier..." -ForegroundColor Green
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    if ($Fix) {
        npx prettier --write "src/**/*.{ts,tsx,js,jsx,html,scss,css,json}" 2>$null
        Write-Host "Code formatted" -ForegroundColor Green
    } else {
        npx prettier --check "src/**/*.{ts,tsx,js,jsx,html,scss,css,json}" 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Formatting issues found (run with -Fix to auto-fix)" -ForegroundColor Yellow
        } else {
            Write-Host "Formatting check passed" -ForegroundColor Green
        }
    }
    $ErrorActionPreference = $previousErrorActionPreference

    # Tests
    if (-not $SkipTests) {
        Write-Host "`nRunning tests..." -ForegroundColor Green
        $previousErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        if ($CoverageReport) {
            npm run test -- --code-coverage --watch=false --browsers=ChromeHeadless
        } else {
            npm run test -- --watch=false --browsers=ChromeHeadless
        }
        $ErrorActionPreference = $previousErrorActionPreference
        
        if ($LASTEXITCODE -ne 0) { 
            Write-Host "Tests failed" -ForegroundColor Red
            exit 1 
        }
        Write-Host "Tests passed" -ForegroundColor Green
    }

    # Build
    Write-Host "`nBuilding for production..." -ForegroundColor Green
    npm run build
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "Build failed" -ForegroundColor Red
        exit 1 
    }
    Write-Host "Build successful" -ForegroundColor Green

    Write-Host "`nFrontend CI checks passed!" -ForegroundColor Green
    exit 0
}
finally {
    Pop-Location
}
