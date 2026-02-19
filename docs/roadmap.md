# Budget Tracker Roadmap (Past → Present → Future)

This roadmap reflects:

- **Planned (explicit)** items from `README.md`
- **Nice-to-have** items implied by `.github/copilot-instructions.md` (especially around CI/CD and Azure demo completeness)

> Maintenance rule: when an Issue is completed or scope changes, update this file and remove the `needs: roadmap-update` label from the corresponding issue(s).

---

## ✅ Done / Delivered (Phase 1 — Demo MVP)

### Product features

- [x] Interactive dashboard (overview of budgets, spending, savings)
- [x] Budget management (create + track monthly budgets by category)
- [x] Transaction tracking (record income and expenses)
- [x] Savings goals (set + monitor goals with progress tracking)
- [x] Charts & visualizations (Chart.js)
- [x] In-memory data (demo/sample data)

### API endpoints (assumed delivered per README)

- [x] Budgets
  - [x] `GET /api/budgets`
  - [x] `GET /api/budgets/{id}`
  - [x] `POST /api/budgets`
- [x] Transactions
  - [x] `GET /api/transactions`
  - [x] `GET /api/transactions/{id}`
  - [x] `POST /api/transactions`
- [x] Savings Goals
  - [x] `GET /api/savings-goals`
  - [x] `GET /api/savings-goals/{id}`
  - [x] `POST /api/savings-goals`

### Architecture

- [x] Frontend: ASP.NET Core MVC (.NET 9)
- [x] Backend: Azure Functions v4 (isolated, .NET 9)

### CI/CD + Azure deployment (already implemented)

- [x] Frontend deploy workflow: `.github/workflows/deploy-frontend.yml`
  - [x] Trigger: push to `main` with changes under `src/frontend/**` (plus manual dispatch)
  - [x] Build + publish + upload artifact
  - [x] Deploy to Azure Web App `red-scus-budget` using OIDC login + `azure/webapps-deploy@v3`

- [x] Backend deploy workflow: `.github/workflows/deploy-backend.yml`
  - [x] Trigger: push to `main` with changes under `src/backend/**` (plus manual dispatch)
  - [x] Build + unit tests + coverage summary + upload test/coverage artifacts
  - [x] Publish output and include `.azurefunctions` metadata for Flex Consumption
  - [x] Deploy to Function App `red-scus-budgetbackend-demo` using OIDC login + `Azure/functions-action@v1`

---

## 📍 Where we are now

A demo-ready full-stack budget tracker with:

- a responsive UI,
- functional API endpoints,
- GitHub Actions workflows that deploy frontend + backend to Azure.

---

## 🔜 Next — Planned (explicit in README.md)

### Platform + architecture

- [ ] Database integration (Cosmos DB or Azure SQL)
- [ ] Infrastructure as Code (Bicep templates)
- [ ] Authentication (Azure Entra External ID / Azure AD B2C)

### Product enhancements

- [ ] Advanced analytics and reporting

---

## 🌟 Nice-to-have — Enhancements (implied by .github/copilot-instructions.md)

### Phase 2 — CI quality gates

- [x] PR CI workflow (build + test for frontend + backend) — `.github/workflows/ci.yml`
- [x] Coverage reporting (results visible in GitHub Step Summary + artifacts)
- [x] Coverage fail thresholds (40% line coverage gate in CI)
- [x] Linting / code analysis gates (`dotnet format --verify-no-changes` in CI)

### Security

- [x] CodeQL workflow (PR + main + scheduled) — `.github/workflows/security.yml`
- [x] Dependency vulnerability scanning (`dotnet list package --vulnerable`)
- [ ] Documented secret scanning expectations

### Phase 3 — CD & environments

- [ ] Multi-environment CD (dev/stage/prod) with environment protection
- [x] Post-deploy smoke tests / health checks (curl-based in deploy workflows)
- [ ] Automatic PR comments with deployment URLs (where applicable)
- [ ] Rollback strategy documentation (redeploy last artifact, slot swap if used)

### Infrastructure validation (once IaC exists)

- [ ] Bicep lint/validate in CI
- [ ] `what-if` preview step for infra changes

### Performance demo

- [ ] Performance testing workflow (manual trigger; e.g., Azure Load Testing)
- [ ] Basic latency baseline capture and documentation

---

## Suggested sequencing (maximize demo impact)

1. Phase 2: CI workflow + CodeQL (fast quality wins for the Actions demo story)
2. Phase 4: IaC with Bicep (repeatable infrastructure + validation/what-if)
3. Phase 5: Database persistence (replace in-memory)
4. Phase 6: Auth (Entra External ID / B2C)
5. Phase 7: Advanced analytics + performance testing