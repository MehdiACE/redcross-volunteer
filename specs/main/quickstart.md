# Quickstart

## Prerequisites
- .NET 10 SDK
- Node.js 20+, npm
- SQL Server (local or Azure SQL)
- Azure Storage account (dev: Azurite/local storage acceptable)
- SendGrid API key; Azure Communication Services (SMS) optional

## Backend (RedCrossManager.Server)
```bash
cd RedCrossManager.Server
# Restore
 dotnet restore
# Run EF migrations (adjust connection string in appsettings.Development.json)
 dotnet ef database update
# Run API
 dotnet run
```
- Base URL: http://localhost:5000 (or as configured)
- Swagger: http://localhost:5000/swagger (dev only)

## Frontend (RedCrossManager.Client)
```bash
cd RedCrossManager.Client
npm install
npm run start  # serves at http://localhost:4200
```

## Environment Configuration
- Backend `appsettings.*.json`: ConnectionStrings:SqlServer; Storage:Blob; Auth:Jwt or AzureAd; SendGrid:ApiKey; Acs:ConnectionString (optional SMS); CORS:AllowedOrigins
- Frontend `.env` or environment.ts: apiBaseUrl, auth endpoints, feature flags (smsEnabled), i18n default lang

## Useful Checks
- Health: `/health` and `/health/ready`
- Database: `dotnet ef migrations add <Name>` then `dotnet ef database update`
- Tests: `dotnet test` (backend), `npm test` (frontend)

## Localization
- Backend: .resx for FR/EN; Accept-Language honored
- Frontend: `src/app/i18n/fr.json`, `en.json`; default French

## CORS
- Dev: http://localhost:4200
- Prod: set explicit origins (no wildcards)
