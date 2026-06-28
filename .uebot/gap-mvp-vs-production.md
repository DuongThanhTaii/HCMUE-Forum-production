# Gap Analysis: MVP vs Production (Forum x UEBot)

## Purpose

This document defines the concrete gap between:
- **MVP readiness**: good enough to validate product value quickly
- **Production readiness**: safe, resilient, scalable, and operable in real traffic

Scope:
- Option A (UEBot Assistant integration)
- Option C (Forum Co-Pilot tools)

---

## 1) Security Gap

### MVP State
- Basic auth works (Forum JWT + sync-api JWT).
- Initial service-to-service exchange can be protected by shared secret.
- Basic rate limiting may exist on selected endpoints.

### Production Requirement
- Strong service authentication (mTLS or signed service tokens).
- Fine-grained authorization checks for assistant tools.
- Strict token lifecycle controls (short-lived access, secure refresh strategy).
- Secret rotation policy and zero hardcoded credentials.
- Security testing (SAST + dependency scanning + abuse scenarios).

### Gap to Close
- Replace minimal shared-secret approach with hardened service auth.
- Add endpoint-level threat model and abuse protection.
- Add automated security checks in CI.

---

## 2) Identity and Access Gap

### MVP State
- Token exchange can map Forum user to sync-api user.
- Minimal role-awareness in assistant context.

### Production Requirement
- Deterministic user mapping with migration-safe schema.
- Multi-device/session control and revocation behavior.
- Clear permission boundaries for moderation and admin-only tools.
- Auditability of who triggered what AI action.

### Gap to Close
- Formalize identity mapping contract (`forum_user_id` strategy).
- Add permission guardrails for each tool endpoint.
- Add full actor audit trail.

---

## 3) Data and Persistence Gap

### MVP State
- Core thread/message persistence available via sync-api.
- Co-Pilot tools can read Forum posts and generate outputs.

### Production Requirement
- Data retention policy (TTL, archival, deletion workflows).
- Backup and restore procedures with tested RPO/RTO.
- PII handling and redaction policy for prompts/logs.
- Migration strategy with rollback safety.

### Gap to Close
- Define data lifecycle policy for assistant conversations.
- Add backup verification and restore drills.
- Add prompt/result data classification and masking.

---

## 4) Reliability and Performance Gap

### MVP State
- Happy-path flows can run end-to-end.
- Limited fallback behavior in failures.

### Production Requirement
- Clear SLOs (availability, latency, error budget).
- Circuit breakers/retries/timeouts between Forum and sync-api.
- Capacity planning and load tests under expected concurrency.
- Graceful degradation strategy when AI providers fail.

### Gap to Close
- Add resilience patterns in all integration paths.
- Define and test peak-load behavior.
- Add deterministic fallback responses in UI and API.

---

## 5) Observability and Operations Gap

### MVP State
- Basic logs and manual debugging.

### Production Requirement
- Structured logs with correlation IDs across services.
- Metrics dashboards (latency, error rate, token exchange success, tool usage).
- Alerting and on-call runbooks.
- Traceability from user action -> backend chain -> provider call.

### Gap to Close
- Implement distributed tracing/correlation IDs.
- Build dashboards and alert thresholds.
- Add incident runbooks for common failure modes.

---

## 6) AI Quality and Safety Gap

### MVP State
- Co-Pilot can generate useful outputs for selected tasks.
- Basic prompt templates and tool orchestration.

### Production Requirement
- Citation policy for fact-based outputs.
- Hallucination mitigation and confidence signaling.
- Prompt/version governance and regression tests.
- Toxic/sensitive output safeguards.

### Gap to Close
- Add output quality gates and citation enforcement.
- Add evaluation suite for summarize/draft/related/moderation_hint.
- Add prompt change review process.

---

## 7) Frontend UX Gap

### MVP State
- Assistant route works, likely iframe-first.
- Basic actions available (summarize, draft, related).

### Production Requirement
- Native integrated UX (reduced iframe dependency).
- Robust state handling (refresh/logout/token-expiry).
- Accessibility, localization, and cross-device consistency.
- Product analytics for funnel and drop-off analysis.

### Gap to Close
- Migrate toward native embed where feasible.
- Harden state synchronization and recovery flows.
- Add UX instrumentation and accessibility checks.

---

## 8) Testing and Release Governance Gap

### MVP State
- Manual and limited integration testing.

### Production Requirement
- Automated test pyramid:
  - unit tests for tools/services
  - integration tests for token exchange and core assistant flows
  - E2E tests for key user journeys
- Release gates and rollback strategy.
- Staging validation checklist.

### Gap to Close
- Expand automated coverage across critical paths.
- Define release criteria and rollback playbook.
- Enforce pre-release validation checklist.

---

## 9) Compliance and Policy Gap

### MVP State
- Minimal policy controls beyond core auth.

### Production Requirement
- Data processing and privacy policy alignment.
- User consent and disclosure for AI-generated content.
- Moderation governance for advisory AI outputs.
- Retention/deletion rights handling.

### Gap to Close
- Add policy artifacts and legal review checkpoints.
- Add explicit AI disclosure in UX.
- Align logs/data retention with platform policy.

---

## Release Readiness Matrix

| Area | MVP Status | Production Target | Priority |
|---|---|---|---|
| Security | Partial | Hardened S2S auth + rotation + tests | P0 |
| Identity | Partial | Deterministic mapping + auditable actions | P0 |
| Data | Partial | Retention + backup/restore + masking | P0 |
| Reliability | Partial | SLO + resilience + load-tested | P0 |
| Observability | Low | Metrics + tracing + alerts | P0 |
| AI Safety | Partial | Citation + eval + safeguards | P1 |
| UX | Partial | Native, resilient, accessible | P1 |
| Testing | Low | Automated gates + rollback playbook | P0 |
| Compliance | Low | Policy-complete and review-ready | P1 |

---

## Recommended Transition Plan (MVP -> Production)

### Phase P0 (Stabilization)
- Secure token exchange path
- Add observability baseline
- Add critical integration tests
- Define SLO and rollback

### Phase P1 (Hardening)
- Add AI quality/safety gates
- Expand performance and resilience tests
- Improve UX integration and failure handling

### Phase P2 (Scale and Governance)
- Capacity planning and cost governance
- Compliance and audit readiness
- Continuous evaluation and incident drills

---

## Exit Criteria for Production Launch

Production go-live should require all of the following:
- No unresolved P0 risks in security/reliability/identity
- Token exchange and assistant tools fully monitored with alerts
- E2E critical journeys passing in staging
- Runbooks and rollback tested
- AI disclosure, moderation policy, and data policy approved
