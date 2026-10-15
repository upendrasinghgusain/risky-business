# Operations

Day-to-day running of the dev and prod environments on the Pi. One-off setup is in [pi-setup.md](pi-setup.md).

Commands marked **on the Pi** run as the `github-runner` user (`sudo -iu github-runner`).

## Deploy

Every push to `main` that passes CI:

1. publishes `ghcr.io/upendrasinghgusain/risky-business-{api,web}:<commit-sha>`,
2. deploys that version to **dev**, then
3. waits for approval to deploy the **same images** to **prod**: open the run in the **Actions** tab and choose **Review deployments**.

Each deployment pulls the images, starts the stack, waits for the containers to become healthy, and smoke-tests `/health/ready` through the web container (web → API → database). Prod is backed up before every deployment.

## Redeploy or roll back

In **Actions → Deploy → Run workflow**, choose the environment and enter the full commit SHA of any version that was previously published. Prod still requires approval.

To find previous versions **on the Pi**:

```bash
cat /opt/riskybusiness/prod/deployments.log
```

Database migrations only move forward. Rolling the application back does not undo a migration, so a migration must stay compatible with the previous application version (for example, add a column in one release and remove the old one in a later release). If a release needs its data changes reversed, restore the backup taken before that deployment.

## Check what is running

- GitHub: **Deployments** on the repository page shows the current version per environment.
- **On the Pi:**

  ```bash
  cat /opt/riskybusiness/prod/version.env
  cd /opt/riskybusiness/prod
  docker compose -p riskybusiness-prod --env-file .env --env-file version.env ps
  docker compose -p riskybusiness-prod --env-file .env --env-file version.env logs --tail 100 api
  ```

## Back up

**On the Pi:**

```bash
/opt/riskybusiness/bin/backup.sh prod
```

Dumps go to `/opt/riskybusiness/backups/<env>/`, and the most recent 30 are kept. A daily prod backup is scheduled by cron (see [pi-setup.md](pi-setup.md)).

## Restore

Replaces **all** of an environment's data. **On the Pi:**

```bash
/opt/riskybusiness/bin/restore.sh prod /opt/riskybusiness/backups/prod/<file>.dump --yes
```

The API is stopped during the restore and started again afterwards.

To practise without touching prod, restore a prod backup into dev:

```bash
/opt/riskybusiness/bin/restore.sh dev /opt/riskybusiness/backups/prod/<file>.dump --yes
```

A backup that has never been restored is unproven, so do this periodically.

## If a deployment fails

The deploy job prints the API and web logs and leaves `version.env` pointing at the last good version. The containers may now be running the failed version, so either push a fix (roll forward) or redeploy the previous version from `deployments.log` (roll back).
