# 0002: Deployment to the Raspberry Pi

- **Status:** Accepted
- **Date:** 2026-10-15

## Context

Stage 2 of the [spec](../spec.md) needs separate dev and prod environments on one Raspberry Pi, a repeatable deployment that promotes the same artefact from dev to prod through a controlled step, backup and restore, and no unnecessary exposure of SSH or management interfaces.

## Decision

- **Artefacts:** CI publishes multi-arch images to GHCR, tagged with the full commit SHA, only after every check passes on `main`. Dev and prod run identical images; nothing is built on the Pi.
- **Deployment mechanism:** a GitHub Actions self-hosted runner on the Pi, labelled `riskybusiness`, running deploy jobs as a dedicated `github-runner` user. It connects outwards to GitHub, so the Pi accepts no inbound connections.
- **Promotion:** dev deploys automatically; prod uses a GitHub environment with a required reviewer. Rollback is a manual run of the Deploy workflow with an earlier SHA.
- **Environments:** one Compose project per environment (`riskybusiness-dev`, `riskybusiness-prod`) under `/opt/riskybusiness/<env>`, each with its own database volume and a `.env` that exists only on the Pi.
- **Ingress:** Cloudflare Tunnel to `127.0.0.1` ports, with HTTPS at `risky-business.upendragusain.co.uk` and `risky-business-dev.upendragusain.co.uk`.
- **Verification:** `docker compose up --wait`, then a smoke test of `/health/ready` through the web container.
- **Backups:** `pg_dump` before every prod deployment and daily by cron, with a scripted restore.
- **Repository visibility:** public, which makes environment required reviewers available on the free plan. Fork pull-request workflows from outside contributors require approval, and deploy jobs never run for pull requests.

## Consequences

- No SSH keys or deployment credentials are stored in GitHub; the workflow's own short-lived token pulls images.
- The `docker` group gives the runner root-equivalent access to the Pi, so the Pi should stay dedicated to this project.
- Migrations still run at API startup, so each migration must be compatible with the previous application version for rollback to be safe.
- Backups live on the Pi until an off-device copy is added.
- Deploys are switched off (repository variable `DEPLOY_ENABLED`) until the runner is installed.
