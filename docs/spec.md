# Risky Business — Project Specification

**Status:** Initial working specification  
**Project:** `risky-business.upendragusain.co.uk`  
**Primary purpose:** Learn a commercial-insurance domain by building a real, evolving software product while practising disciplined software engineering and delivery.

## 1. Vision and intent

Risky Business is a long-running personal engineering project centred on a narrow slice of commercial insurance distribution and processing. It may eventually explore a multi-tenant SaaS model, but commercialisation is a hypothesis to investigate—not an assumed outcome.

The project has two equally important goals:

1. **Domain learning:** progressively understand insurance actors, workflows, terminology, rules, exceptions and operational realities by researching, modelling, implementing and testing them.
2. **Engineering excellence:** build and maintain a codebase using practices suitable for a serious production system, including version control, review, automated testing, repeatable delivery, observability, security, resilience and measured performance.

The project should grow through small, working increments. Each new layer of technical complexity must have a clear learning or business purpose. Avoid building an enterprise platform in advance of understanding the domain.

## 2. Guiding principles

- **Domain first, architecture earned.** Start with a real business journey and progressively deepen its rules and edge cases. Let architecture evolve from evidence.
- **Delivery discipline from day one.** The first feature must go through version control, pull-request review, automated checks and a repeatable deployment path.
- **Start thin, not throwaway.** Prefer a small working vertical slice with tests and a maintainable structure over either a disposable prototype or a speculative framework.
- **Use free/open-source tools initially.** Prefer tools that can be self-hosted on a Raspberry Pi or run on free tiers. Be explicit about limits, maintenance effort and any future costs.
- **Terraform is not a starter requirement.** Introduce Terraform when the project needs real cloud resources or repeatable infrastructure provisioning. Do not use it merely to provision the initial local/Pi setup.
- **Modular monolith first.** Keep modules and boundaries clear, but do not begin with microservices, Kubernetes or a distributed architecture.
- **Measure before optimising.** Establish performance and reliability targets, load-test, inspect bottlenecks and make evidence-based changes.
- **Learn by deliberately testing failure.** Use controlled dependencies, fault injection and recovery exercises as the system becomes more complex.
- **Keep environments separate.** Development and production may initially share one Pi, but must have separate configuration, data, credentials and deployment controls.
- **Make security incremental but not optional.** Protect secrets and the deployment path from the start. Add user identity and application authorisation before introducing real multi-user data.
- **Record uncertainty.** Distinguish confirmed domain facts from assumptions, market-specific variations and open questions.
- **Protect independent work.** Use personal equipment, accounts, time and synthetic data. Do not copy employer code, confidential information, schemas or customer data. Review contractual/IP obligations and obtain independent advice before commercialising if needed.

## 3. Initial scope and architecture

The initial implementation should be deliberately small:

- **Frontend:** Angular single-page application.
- **Backend:** ASP.NET Core Web API using C#.
- **Persistence:** one relational database, such as PostgreSQL or SQL Server, selected for ease of local operation and testing.
- **Local runtime:** Docker and Docker Compose.
- **Tests:** .NET unit and integration tests, frontend tests, and a small number of end-to-end tests when a full journey exists.
- **Source control:** Git repository, preferably a monorepo initially.
- **CI/CD:** GitHub Actions or another suitable free/open-source-compatible pipeline, using a hosted free runner where available or a self-hosted runner if appropriate.
- **Deployment target:** Raspberry Pi with a 64-bit OS and compatible container images.
- **Environments:** separate dev and prod Compose deployments, with separate configuration, volumes/databases and credentials.
- **Observability:** structured application logs and health checks from the beginning; richer telemetry later.
- **External integrations:** start with controlled fake adapters and synthetic data.

Do not add a message broker, cache, multiple databases, microservices, Kubernetes, a runtime plugin system or a complex identity stack until a concrete requirement or learning experiment justifies it.

A modular monolith is the default. Keep domain rules separated from infrastructure concerns and use explicit module interfaces where useful. Avoid turning every business concept into a separate service or database table prematurely.

## 4. Domain-learning strategy

The domain should grow from a narrow, end-to-end workflow rather than a broad speculative model of the whole insurance industry.

A possible initial journey is:

1. A fictional broker submits a small commercial risk.
2. The API validates the submission.
3. Broker/product access is checked using simple, explicit rules.
4. A controlled external risk-data adapter enriches the submission.
5. A basic, fictional rating model calculates a premium.
6. Underwriting rules decide whether to proceed or refer.
7. A quote is produced with terms, premium, expiry and an auditable record of the decision.

This is a learning scenario, not a claim that all insurers or brokers follow the same workflow. Keep all data synthetic and clearly label invented rating factors and rules.

Progressively investigate and model:

- People, organisations and their different roles.
- Broker, insurer, MGA, Lloyd’s-related actors and distribution relationships where relevant.
- Product access, appointments, eligibility, delegated authority and who may submit, quote, underwrite or bind.
- Submission, risk, exposure, rating, underwriting referral, quote, policy and renewal lifecycle.
- Effective dates, versioned rules, amendments, expired or suspended access and in-flight work.
- External dependencies, operational exceptions, auditability and recovery.

Do not assume that organisational hierarchy, commercial relationships, product access and delegated authority are interchangeable. Identify where practices vary by market, product, insurer or distribution arrangement.

Maintain these lightweight domain artefacts:

- Glossary of terms.
- Workflow and relationship maps.
- Confirmed rules and their evidence/source.
- Assumptions and market-specific variations.
- Open questions for domain experts.
- Decision records for material modelling choices.
- A list of hypotheses that could be disproved by further research.

The personal project is a laboratory, not a substitute for learning from real workflows, experts, approved materials and incidents.

## 5. Engineering and delivery standards

These practices are first-class concerns from the start, even if the first implementation is minimal.

### Source control and review

- Commit source code, tests, pipeline definitions, deployment configuration and documentation.
- Use small feature branches and pull requests.
- Protect the main branch and require automated checks to pass.
- Use meaningful commit/PR descriptions and review the change before merging.
- Avoid secrets, local state, build outputs and production data in Git.

### Continuous integration

The PR pipeline should progressively include:

- Formatting and static analysis.
- Frontend and backend builds.
- Unit tests.
- Architecture tests where useful.
- Integration tests using disposable dependencies.
- Dependency and container vulnerability checks where practical.
- Terraform formatting, validation and plan checks once Terraform is introduced.
- Test reports and useful failure output.

Keep PR feedback fast and deterministic. Run expensive, slow or environment-dependent tests separately when appropriate.

### Continuous delivery

- Build a versioned container image from a known commit.
- Promote the same image artefact between environments rather than rebuilding different images for dev and prod.
- Deploy to dev automatically after the agreed integration point.
- Verify deployments with smoke tests and health checks.
- Promote to prod through an explicit, controlled step.
- Record which version is running in each environment.
- Practise rollback or roll-forward, including database compatibility considerations.
- Keep deployment credentials narrow in scope and avoid exposing SSH or management interfaces unnecessarily.

Continuous delivery means keeping changes releasable; it does not require every commit to be deployed automatically to production.

### Environment and infrastructure management

Initially, use Compose and versioned scripts/configuration to operate the Pi. Dev and prod should have separate configuration, persistent data and credentials. Document the deployment and recovery process.

Introduce Terraform when real cloud infrastructure is required or manual infrastructure setup becomes error-prone. At that point:

- Store configuration in Git.
- Use reusable modules only when there is a genuine need.
- Separate state by environment and protect it.
- Review plans before applying.
- Restrict production apply permissions.
- Never commit state files or secrets.
- Verify provisioned resources and test teardown where safe.

Terraform does not automatically replace host configuration management or deployment scripts for every aspect of the Pi.

### Testing strategy

Use the right test for the risk:

- **Unit tests:** business rules and pure logic.
- **Component tests:** a component and its immediate collaborators.
- **Integration tests:** real interaction with a database or other dependency.
- **Contract tests:** compatibility across independently owned interfaces.
- **End-to-end tests:** a small number of important user journeys.
- **Architecture tests:** structural dependency rules.
- **Security tests:** authentication, authorisation, tenant isolation and input handling.
- **Deployment smoke tests:** verify the deployed service is operational.
- **Performance/load tests:** quantify throughput, latency and resource limits.
- **Resilience tests:** timeouts, dependency failure, retries, duplicate messages and recovery.
- **Infrastructure checks:** validate plans and verify actual resources when IaC is introduced.

Do not rely on a large E2E suite as the only safety net. Keep normal CI independent of unstable third-party systems by using controlled fakes or test dependencies where appropriate.

### Operations and continuous improvement

- Add structured logs and health checks from the beginning.
- Add traces, metrics, dashboards and alerts as complexity grows.
- Maintain concise runbooks for deployment, rollback, backup and restore.
- Test restoring backups; a backup that has never been restored is unproven.
- Keep a short engineering journal of failed deployments, defects, test gaps, bottlenecks and lessons learned.
- Periodically simplify the system and remove unnecessary complexity.

## 6. Progressive roadmap

The roadmap is intentionally incremental. Move forward when the current stage works reliably enough to support the next; do not treat the stage list as a fixed calendar.

### Stage 0 — Repository and working agreements

**Build:** repository, initial `spec.md`, README, domain notes, architecture decision record template, `.gitignore`, branch/PR rules and a small backlog.

**Exit criteria:**
- The project purpose and constraints are documented.
- Changes are made through reviewed PRs.
- No secrets or private employer material are stored in the repository.

### Stage 1 — Minimal application and CI

**Build:** Angular SPA, ASP.NET Core API, relational database, Docker Compose and one trivial but working vertical slice.

**Pipeline:** build, formatting/static checks, unit tests and integration tests.

**Exit criteria:**
- A change can be made in a branch, reviewed, merged and built reproducibly.
- Tests run automatically and report failures clearly.
- The app can be started consistently from documented instructions.

### Stage 2 — Pi dev/prod and repeatable delivery

**Build:** separate dev and prod Compose deployments on the Pi, environment-specific configuration, persistent data, HTTPS/tunnel routing as needed, backup/restore and scripted deployment.

**Pipeline:** deploy dev, run smoke tests, and promote the same image to prod through a controlled step.

**Exit criteria:**
- Deployments do not depend on undocumented manual steps.
- Dev and prod data and credentials are separated.
- A previous application version can be restored, and a database backup can be recovered.

### Stage 3 — First meaningful insurance workflow

**Build:** a narrow submission-to-quote journey, domain glossary, explicit business rules, synthetic data and tests.

**Exit criteria:**
- The user can complete the journey.
- Important rules and exceptions are documented and tested.
- Unknown domain assumptions are visible rather than disguised as facts.

### Stage 4 — Domain depth and stronger quality gates

**Build:** more nuanced product eligibility, broker access, effective dates, referrals, rule versioning and audit records as justified by domain research.

**Pipeline:** stronger integration and contract tests, architecture checks, dependency scanning and selected security tests.

**Exit criteria:**
- Changes to domain rules are traceable and regression-tested.
- Interfaces and module boundaries are deliberate.
- The project can explain why a decision was made, not merely what result was produced.

### Stage 5 — Identity, authorisation and tenant isolation

**Build:** an OIDC identity provider such as Keycloak or ZITADEL; registration/login as required; application-side users, organisations, memberships and permissions.

**Tests:** authenticated API tests, role checks and explicit cross-tenant access-denial tests.

**Exit criteria:**
- Authentication and application authorisation are distinct.
- A user cannot access another tenant's data by changing an identifier or request.
- Identity-provider configuration and secrets are handled safely.

### Stage 6 — Observability and integration resilience

**Build:** OpenTelemetry traces/metrics, useful dashboards, dependency adapters, timeouts, bounded retries, idempotency and failure simulation.

**Exit criteria:**
- A failure can be diagnosed from telemetry.
- External dependency failure has an explicit user/business outcome.
- Recovery and retry behaviour is tested rather than assumed.

### Stage 7 — Messaging and asynchronous processing

**Build:** introduce a message broker only for a suitable workflow, plus a background worker. Explore the outbox pattern, duplicate delivery, idempotent consumers, dead-letter handling and queue monitoring.

**Exit criteria:**
- Work can be processed asynchronously and recovered safely.
- Duplicate messages do not create incorrect business outcomes.
- Queue backlog and processing failures are observable.

### Stage 8 — Measured performance and scale-out

**Build:** repeatable load tests, performance baselines, profiling, database tuning, caching where justified, concurrency limits and backpressure.

Then move a deployment to cloud compute and managed services if needed to practise multiple API instances and independently scaled workers.

**Exit criteria:**
- There is a stated workload target and measured result.
- Bottlenecks are identified using evidence.
- Scale-out and failure behaviour are tested.
- The project distinguishes demonstrated capacity from aspirations.

### Stage 9 — Cloud infrastructure as code

**Trigger:** real cloud resources are needed for the next learning goal or for a more realistic deployment.

**Build:** Terraform for selected cloud infrastructure, environment-specific state, reviewed plans, restricted applies, drift checks and documented teardown/recovery.

**Exit criteria:**
- Infrastructure changes are reviewed and repeatable.
- State and credentials are protected.
- Cloud spend is monitored and resources can be cleaned up safely.

### Stage 10 — Mature SaaS operations and optional advanced platform work

**Build when justified:** tenant provisioning, admin portal, rate limits/quotas, richer audit trails, disaster-recovery exercises, cost controls, deployment strategies and service extraction experiments.

Kubernetes, multi-region architecture and microservices are optional learning experiments, not required milestones. Add them only when there is a concrete question to investigate and a way to measure what was learned.

## 7. Definition of success

The project is progressing well when:

- The insurance domain becomes more accurate and nuanced over time.
- The user can explain the business workflow, the authority behind decisions and important exceptions.
- Features are delivered through repeatable, reviewed and tested pipelines.
- Dev and prod are distinct and can be recovered.
- Security, observability and resilience are tested capabilities, not just installed tools.
- Performance claims are backed by repeatable measurements.
- New technologies are introduced for a reason and can be removed if they do not earn their complexity.
- The user can explain both the benefits and trade-offs of the engineering decisions made.

The aim is not to maximise the number of technologies used. It is to build a credible, evolving insurance application and an equally credible engineering system around it.

## 8. Immediate next steps

1. Create the repository and commit this specification.
2. Agree on a small initial stack and folder structure.
3. Add PR protection and a basic CI pipeline before implementing substantial functionality.
4. Create the smallest Angular/.NET/database Compose setup.
5. Deploy it to the Pi through a documented process.
6. Establish separate dev and prod Compose configurations and prove a safe promotion/rollback path.
7. Begin the insurance domain foundation: personas, relationships, glossary, lifecycle and open questions.
8. Choose one small business workflow and implement it through the full delivery pipeline.

**Do not begin with Terraform, Kubernetes, microservices or a complex SaaS control plane. Begin with a disciplined delivery loop and a small insurance-domain slice, then let demonstrated needs drive the next increment.**
