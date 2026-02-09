#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs full CI pipeline (backend + frontend checks)
.DESCRIPTION
    Complete quality assurance for the repository
#>

param(
    [switch]$Fix,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "  Full Repository CI Pipeline" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

$startTime = Get-Date

# Backend CI
Write-Host "Stage 1: Backend Checks" -ForegroundColor Cyan
& ".\scripts\backend-ci.ps1" -Fix:$Fix -SkipTests:$SkipTests
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBackend CI failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Frontend CI
Write-Host "Stage 2: Frontend Checks" -ForegroundColor Cyan
& ".\scripts\frontend-ci.ps1" -Fix:$Fix -SkipTests:$SkipTests
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nFrontend CI failed" -ForegroundColor Red
    exit 1
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host "`n================================" -ForegroundColor Green
Write-Host "ALL CI CHECKS PASSED" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host "Total time: $([Math]::Round($duration, 2))s`n" -ForegroundColor DarkGray

exit 0
