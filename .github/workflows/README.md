# GitHub Actions Workshop Guide

Five workflows demonstrate automated quality checks, Azure deployments, security scanning, and AI-powered repository maintenance.

| Workflow | When it runs | What it demonstrates |
| --- | --- | --- |
| [`ci.yml`](ci.yml) | Pull requests targeting `main`; pushes to other branches | **Quality checks before merge.** Builds and tests frontend/backend in parallel, enforces **40% line coverage**, uploads test and coverage reports, checks .NET formatting, and validates Bicep files. |
| [`deploy-backend.yml`](deploy-backend.yml) | Backend changes pushed to `main`, or manually | **API delivery pipeline.** Builds and tests the Azure Functions backend, generates coverage reports, packages it for Flex Consumption, and deploys through the `prod` environment using **OIDC authentication**. Finishes with an `/api/budgets` smoke test. |
| [`deploy-frontend.yml`](deploy-frontend.yml) | Frontend changes pushed to `main`, or manually | **Web application delivery.** Builds and publishes the MVC app, passes the artifact to a separate deployment job, authenticates with OIDC, deploys to Azure App Service production, and smoke-tests the site. |
| [`security.yml`](security.yml) | Pull requests to `main`, pushes to `main`, and Mondays at 06:25 UTC | **Continuous security feedback.** Runs C# CodeQL analysis and checks direct/transitive NuGet dependencies for known vulnerabilities. Dependency findings appear in the run summary but **do not fail the workflow**. |
| [`issue-triage-agent.lock.yml`](issue-triage-agent.lock.yml) | Weekdays at 14:00 UTC, or manually | **AI-powered repository maintenance.** Uses Copilot to classify unlabeled, unassigned issues, apply an allowed label, and comment with reasoning and suggested next steps. Uses restricted tools and controlled write operations. Generated from [`issue-triage-agent.md`](issue-triage-agent.md). |

## Workshop Talking Points

- **Parallel jobs:** Frontend and backend quality checks run independently to shorten feedback time.
- **Artifacts:** Build outputs and test reports persist between jobs or remain available after a run.
- **Path filters:** Each deployment runs automatically only when its application files or its workflow change on `main`.
- **OIDC:** Azure deployments use federated authentication instead of a stored client secret.
- **Environments:** Both deployments use the GitHub `prod` environment; approval rules must be configured separately in repository settings.
- **Smoke tests:** Deployment workflows check for a successful HTTP response after deployment.

## Demo Boundaries

- Frontend deployment relies on CI for unit tests; its deployment workflow does not run them.
- Deployment workflows target existing Azure resources rather than provisioning infrastructure.
- CI validates Bicep files locally; it does not run an Azure deployment preview or `what-if`.
- The issue-triage agent uses lockdown mode to restrict processing to trusted issue content. Edit its Markdown source and run `gh aw compile` to regenerate the lock workflow.
