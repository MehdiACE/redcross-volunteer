# CI/CD Documentation

## Overview

This document describes the Continuous Integration and Continuous Deployment setup for the Red Cross Volunteer Manager system.

## GitHub Actions Workflows

### 1. Backend CI (`backend.yml`)

Runs on push/PR to main branches. Includes:

- **Build & Test Job**
  - Restores .NET dependencies
  - Builds solution (Release configuration)
  - Runs unit tests with code coverage collection
  - Uploads test results and coverage artifacts

- **Lint Job (StyleCop)**
  - Enforces code style rules
  - Fails build if violations detected

- **Security Scan Job**
  - Checks for vulnerable dependencies via Dependabot
  - Reports vulnerable packages

**Requirements**:
- .NET 10.x SDK
- StyleCop analyzer rules in project files
- Test projects configured with coverage collectors

### 2. Frontend CI (`frontend.yml`)

Runs on push/PR to main branches. Includes:

- **Build & Test Job**
  - Installs Node dependencies (npm ci)
  - Runs linting (ESLint)
  - Executes tests with Karma/Jasmine
  - Builds production artifacts
  - Uploads coverage and build artifacts

- **Lint Job (ESLint)**
  - Runs ESLint for code quality
  - Runs Prettier for format checking

- **Security Scan Job**
  - Audits npm packages for vulnerabilities
  - Reports moderate and above severity issues

**Requirements**:
- Node.js 20.x
- ESLint/Prettier configured
- Karma test runner configured
- Angular CLI available

### 3. Full CI Pipeline (`ci.yml`)

Orchestrates both backend and frontend workflows:

- Runs backend and frontend jobs in parallel
- Collects coverage reports
- Performs PR compliance checks
- Enforces PR template usage

**Triggers**:
- Push to main, develop, or 001-volunteer-onboarding branches
- Pull requests to main or develop

## Local Development Scripts

Run comprehensive CI checks locally before pushing:

### All Checks
```bash
npm run ci              # Run full pipeline
npm run ci:fix          # Fix formatting and lint issues
```

### Backend Only
```bash
npm run ci:backend      # Backend build, lint, tests
npm run ci:backend:fix  # Fix code style with dotnet-format
```

### Frontend Only
```bash
npm run ci:frontend     # Frontend lint, tests, build
npm run ci:frontend:fix # Auto-fix ESLint and Prettier issues
```

### PowerShell Scripts
```powershell
# Full pipeline
.\scripts\ci.ps1 -Fix -SkipTests

# Backend only
.\scripts\backend-ci.ps1 -Fix -CoverageReport

# Frontend only
.\scripts\frontend-ci.ps1 -Fix -CoverageReport
```

## Coverage Requirements

### Backend (.NET)
- **Target**: ≥80% line coverage for services and repositories
- **Enforcement**: Coverage metrics collected in GitHub Actions
- **Tools**: XPlat Code Coverage collector

### Frontend (Angular)
- **Target**: ≥80% line coverage for services and components
- **Enforcement**: Coverage metrics collected in GitHub Actions
- **Tools**: Karma coverage reporter

## Code Quality Standards

### Backend (C#/.NET)

1. **StyleCop Rules**
   - Enforced via build-time rules
   - See `Directory.Build.props` for configuration
   - Failure blocks build in CI

2. **Naming Conventions**
   - PascalCase for public members and types
   - camelCase for private/internal fields
   - CONSTANT_CASE for constants

3. **Documentation**
   - XML comments on public APIs
   - Comment ratio: ≥5 lines per public method

### Frontend (TypeScript/Angular)

1. **ESLint Configuration**
   - Angular linting rules enabled
   - Security and best practices enforced
   - Warnings treated as warnings (non-blocking in CI for now)

2. **Prettier Formatting**
   - 2-space indentation
   - Single quotes for TypeScript
   - Print width: 100 characters

3. **Testing Standards**
   - All components/services must have `.spec.ts` files
   - Minimum 80% coverage for new code
   - Use Jasmine/Karma test runner

## Security Scanning

### Dependency Vulnerabilities
- **Backend**: `npm audit` style checking via dotnet
- **Frontend**: `npm audit --audit-level=moderate`
- **Frequency**: On every build (fails if moderate+ vulnerabilities found)

### Secret Detection
- GitHub's native secret scanning enabled
- Consider using: git-secrets, TruffleHog for enhanced scanning

### Code Review Requirements
- At least 1 approval required before merge
- All CI checks must pass
- PR template must be completed

## PR Template Compliance

Located at `.github/pull_request_template.md`

Ensures PRs include:
- Clear description and related issues
- Type of change documented
- Testing confirmation
- Code quality checklist
- Security & compliance verification
- Database migration notes (if applicable)
- Documentation updates

## Performance Gates

### Build Time
- Backend: Target <5 minutes
- Frontend: Target <3 minutes
- Full pipeline: Target <10 minutes

### Test Execution
- Backend tests: Target <2 minutes
- Frontend tests: Target <1 minute

## Failure Handling

### When CI Fails

1. **Backend failures**
   - Check build output for StyleCop violations
   - Run `npm run ci:backend:fix` locally to auto-fix
   - Review test failures in artifact

2. **Frontend failures**
   - Check ESLint/Prettier output
   - Run `npm run ci:frontend:fix` locally
   - Review test failures in artifact

3. **Coverage failures**
   - Run tests with coverage report
   - Identify uncovered code paths
   - Add tests for new code

## Secrets Management

- Store secrets in GitHub repository settings
- Never commit `.env` files or secrets
- Use Azure Key Vault reference in deployment configs
- CI pipelines access secrets via `${{ secrets.SECRET_NAME }}`

## Branch Protection Rules

Configure for `main` and `develop` branches:
- Require PR reviews (≥1)
- Require status checks to pass (CI pipeline)
- Dismiss stale PR approvals
- Require branches to be up to date

## Deployment Pipeline

(To be configured based on deployment strategy)

- Manual approval gates for production
- Automated deployment to staging
- Health check verification post-deployment
- Rollback strategy documented

## Monitoring & Alerts

### CI Metrics to Track
- Build success rate (target: >95%)
- Average build duration trend
- Test coverage trend
- Flaky test detection
- Dependency update frequency

### Notifications
- Slack notifications for build failures (to be configured)
- Email notifications for merge conflicts

## Future Enhancements

- [ ] Docker image building and registry push
- [ ] Performance benchmarking on each commit
- [ ] Load testing on staging deployments
- [ ] Security scanning with SonarQube
- [ ] Contract testing for API changes
- [ ] Database migration testing with Testcontainers
- [ ] Mobile (PWA) testing
