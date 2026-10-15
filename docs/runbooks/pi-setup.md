# Raspberry Pi setup

One-off setup that turns a Pi with a fresh 64-bit OS into the dev and prod host. After this, deployments run from GitHub Actions (see [operations.md](operations.md)).

How it fits together:

```text
push to main ──► CI (GitHub-hosted) ──► images on ghcr.io, tagged with the commit SHA
                                               │
GitHub runner on the Pi (outbound only) ◄──────┘  deploy dev automatically, prod after approval
        │
        ├─ riskybusiness-dev   web on 127.0.0.1:8181 ─┐
        └─ riskybusiness-prod  web on 127.0.0.1:8180 ─┼─ cloudflared (outbound only) ──► Cloudflare ──► HTTPS
                                                      │
          risky-business-dev.upendragusain.co.uk ─────┘  risky-business.upendragusain.co.uk
```

Nothing on the Pi accepts connections from the internet; the runner and tunnel both connect outwards.

## 1. Check the OS

```bash
uname -m                      # must print aarch64 (64-bit)
sudo apt update && sudo apt full-upgrade -y
```

## 2. Install Docker

Docker's official install script adds its apt repository and installs Docker Engine and the Compose plugin:

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
less get-docker.sh            # read it before running it
sudo sh get-docker.sh
docker compose version        # check Compose is available
```

## 3. Create the deployment user and directories

The runner runs as its own user rather than yours. Membership of the `docker` group is effectively root on this machine, so keep the Pi dedicated to this project.

```bash
sudo adduser --disabled-password --gecos "" github-runner
sudo usermod -aG docker github-runner

sudo mkdir -p /opt/riskybusiness/{dev,prod,backups}
sudo chown -R github-runner:github-runner /opt/riskybusiness
```

## 4. Create each environment's secrets

Each environment gets its own password, which only ever exists on the Pi. Never reuse the `local-dev-only` password from the repository.

```bash
sudo -u github-runner bash -c '
  umask 077
  printf "POSTGRES_PASSWORD=%s\nWEB_PORT=8181\n" "$(openssl rand -hex 24)" > /opt/riskybusiness/dev/.env
  printf "POSTGRES_PASSWORD=%s\nWEB_PORT=8180\n" "$(openssl rand -hex 24)" > /opt/riskybusiness/prod/.env
'
```

The password is applied when an environment's database volume is first created. Changing it in `.env` later does not change the existing database's password.

## 5. Install the GitHub Actions runner

1. On GitHub, open **Settings → Actions → Runners → New self-hosted runner** for this repository and choose **Linux** and **ARM64**.
2. Run the download and `config.sh` commands it shows **as `github-runner`**, in that user's home directory (`sudo -iu github-runner`). When `config.sh` asks:
   - runner name: `pi`
   - additional labels: `riskybusiness` (the deploy workflow targets this label)
3. Install it as a service, so it starts on boot (run from the runner directory, as your normal user):

   ```bash
   sudo ./svc.sh install github-runner
   sudo ./svc.sh start
   ```

The runner should now show as **Idle** in the repository's runner settings.

Because the repository is public, deploy jobs only run for pushes to `main` and manual runs, never for pull requests, and workflows from first-time contributors' pull requests wait for approval.

## 6. Set up the Cloudflare Tunnel

Requires the `upendragusain.co.uk` DNS to be managed by Cloudflare (free plan).

1. In the Cloudflare dashboard, go to **Zero Trust → Networks → Tunnels → Create a tunnel**, choose **Cloudflared**, and name it `pi`.
2. Choose **Debian / arm64** and run the install command it shows on the Pi. This installs `cloudflared` as a service using the tunnel token.
3. Add two **public hostnames** to the tunnel:

   | Hostname | Service |
   |---|---|
   | `risky-business.upendragusain.co.uk` | `http://localhost:8180` |
   | `risky-business-dev.upendragusain.co.uk` | `http://localhost:8181` |

   Use `risky-business-dev`, not `dev.risky-business`: the free certificate covers only one level of subdomain.
4. Recommended: keep dev private with **Zero Trust → Access → Applications**, adding a self-hosted application for `risky-business-dev.upendragusain.co.uk` that only allows your email address.

## 7. Turn on deployments

From a machine with the GitHub CLI:

```bash
gh variable set DEPLOY_ENABLED --body true --repo upendrasinghgusain/risky-business
```

The next push to `main` deploys dev and then waits for approval to deploy prod. To deploy the current version straight away, run the **Deploy** workflow by hand (see [operations.md](operations.md)).

## 8. Schedule backups

As `github-runner` (`sudo crontab -u github-runner -e`):

```cron
15 2 * * * /opt/riskybusiness/bin/backup.sh prod >> /opt/riskybusiness/backups/backup.log 2>&1
```

`/opt/riskybusiness/bin` is installed by the first deployment. Backups are kept on the Pi only, so also copy them off the device regularly (for example to another machine or cloud storage), and practise a restore (see [operations.md](operations.md)).
