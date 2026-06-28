# Production Gates Tasklist (Go-Live Checklist)

## Gate 1 - Security (P0)

- [ ] Internal exchange endpoint not publicly exposed
- [ ] Service credential rotation procedure documented
- [ ] Token exchange endpoint rate-limited and monitored
- [ ] Security scan pipeline enabled for both Forum and UEBot
- [ ] No unresolved critical/high vulnerability

## Gate 2 - Reliability (P0)

- [ ] Timeout/retry/circuit-breaker behavior validated
- [ ] Load test executed at expected peak concurrency
- [ ] Graceful degradation confirmed when AI provider fails
- [ ] Incident runbook for top failure modes available

## Gate 3 - Data and Compliance (P0/P1)

- [ ] Retention policy defined for assistant messages/logs
- [ ] Backup and restore test completed successfully
- [ ] PII masking/redaction policy applied to logs
- [ ] AI output disclosure copy approved in UI/legal text

## Gate 4 - Observability (P0)

- [ ] Correlation IDs visible across Forum -> sync-api chain
- [ ] Dashboards include:
  - [ ] token exchange success rate
  - [ ] tool endpoint latency/error
  - [ ] assistant usage trend
- [ ] Alerts configured and tested

## Gate 5 - Quality (P0)

- [ ] Unit/integration/E2E critical suites pass
- [ ] Staging sign-off from backend + frontend owners
- [ ] Rollback plan tested on staging
- [ ] Release notes and known limits documented

## Final Go/No-Go Decision

- [ ] Product owner approval
- [ ] Engineering lead approval
- [ ] Security/platform approval
- [ ] Launch window and on-call assignment confirmed
