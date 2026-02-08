# PR Title: [Feature/Fix/Chore] - Brief description

## Description
<!-- Provide a clear description of the changes in this PR -->

## Related Issues
<!-- Link to related issues (e.g., Closes #123, Related to #456) -->

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Test coverage ≥80% for affected services/repositories (enforced by CI)
- [ ] Manual testing performed

## Constitution Compliance
- [ ] **Architecture**: Follows layered architecture (Repository/Service/Controller/DTO)
- [ ] **TDD**: Tests written before implementation
- [ ] **Coverage Gate**: CI enforces ≥80% line coverage for services/repositories
- [ ] **Multilingual**: FR/EN translations added (.resx and ngx-translate)
- [ ] **Cloud-ready**: Compatible with Azure App Service, SQL, Blob, Key Vault
- [ ] **Security-first**: JWT/RBAC enforced, input validation, no secrets in code

## Code Quality Checklist
- [ ] Code follows project style guide (StyleCop, ESLint, Prettier)
- [ ] No console.log() or debug code left
- [ ] Logging is appropriate and PII-safe
- [ ] Validation is comprehensive (backend + frontend)
- [ ] Error handling is implemented
- [ ] Comments added for complex logic

## Security & Compliance
- [ ] No secrets/credentials exposed in code
- [ ] No new dependencies with known vulnerabilities
- [ ] CORS/Auth policies verified
- [ ] Input validation implemented
- [ ] RBAC checks added where appropriate

## Accessibility & Internationalization
- [ ] Accessible markup (ARIA labels, focus states)
- [ ] i18n keys added to en.json and fr.json
- [ ] No hardcoded English strings

## Database Changes (if applicable)
- [ ] Migration created and tested
- [ ] Migration is reversible
- [ ] Seed data updated if needed
- [ ] Data loss scenarios documented

## Documentation
- [ ] README updated if needed
- [ ] API contracts versioned (/api/v1)
- [ ] Architecture decisions documented
- [ ] Complex logic has explanatory comments

## CI/CD
- [ ] Backend CI passes (lint, build, tests)
- [ ] Frontend CI passes (lint, tests, build)
- [ ] No performance regressions
- [ ] No console errors/warnings introduced

## Reviewer Notes
<!-- Add any additional information for reviewers -->

---

**Constitution Compliance**: This PR adheres to our architecture principles:
- [x] Layered architecture maintained (Repository/Service/Controller/DTO)
- [x] Tests written first (TDD approach)
- [x] Security-first mindset applied
- [x] Multilingual support maintained
- [x] Cloud-ready code (no local dependencies)
