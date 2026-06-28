# Full Implementation Tasklist (Forum x UEBot)

## How to Use

- Use this file as the top-level checklist.
- Execute tasks in order by phase.
- Do not move to next phase until all P0 items in current phase are completed.

---

## Phase 0 - Alignment and Contracts (P0)

- [ ] Confirm integration scope: Option A (Assistant module) + Option C (Co-Pilot tools)
- [ ] Freeze API contract for token exchange:
  - [ ] `POST /api/v1/integrations/uebot/exchange-token` (Forum)
  - [ ] `POST /integrations/forum/exchange` (sync-api internal)
- [ ] Freeze response schema for assistant tools endpoints
- [ ] Agree auth model for iframe/native embed session
- [ ] Agree ownership boundaries:
  - [ ] Forum team (identity/tools/UI integration)
  - [ ] UEBot team (sync-api + web-app auth)

**Exit criteria**
- [ ] Contract docs approved by backend + frontend owners
- [ ] No open blocker on identity mapping strategy

---

## Phase 1 - Option A MVP Foundation (P0)

### Backend integration
- [ ] Implement Forum token exchange endpoint
- [ ] Implement sync-api trusted exchange endpoint
- [ ] Add secure service auth (minimum shared secret)
- [ ] Add config entries for base URL + secret
- [ ] Add rate limiting for integration endpoints

### Frontend integration
- [ ] Add `/assistant` route in Forum frontend
- [ ] Build assistant shell page (iframe phase)
- [ ] Implement postMessage token handoff
- [ ] Implement token refresh propagation
- [ ] Implement logout/session clear synchronization

### UEBot readiness
- [ ] Complete `useAuth` missing methods in UEBot web-app
- [ ] Ensure auth-required dialog is fully wired
- [ ] Validate guest -> user merge behavior after login

**Exit criteria**
- [ ] User can login Forum, open Assistant, create thread/message, refresh page, and keep state
- [ ] No P0 security issue in token exchange flow

---

## Phase 2 - Option C Co-Pilot Core Tools (P0/P1)

### Backend tools API
- [ ] Create `POST /api/v1/assistant/tools/summarize-post`
- [ ] Create `POST /api/v1/assistant/tools/related-posts`
- [ ] Create `POST /api/v1/assistant/tools/draft-reply`
- [ ] Add request validation for all tool payloads
- [ ] Add response envelope consistency and error code mapping

### Data + service integration
- [ ] Hook `summarize-post` to post + comments + AI summarization service
- [ ] Hook `related-posts` to real Forum search (no mock path in prod flow)
- [ ] Hook `draft-reply` to context retrieval + prompt template
- [ ] Add citations field for fact-based outputs

### Frontend features
- [ ] Add "Summarize post" action on post detail
- [ ] Add "Suggest reply" action on post detail
- [ ] Add "Related posts" panel
- [ ] Add "Insert draft" UX with explicit user confirmation

**Exit criteria**
- [ ] Three core tools work end-to-end from Forum UI
- [ ] Responses are traceable with request IDs and proper logs

---

## Phase 3 - Safety, Reliability, and Observability (P0)

- [ ] Add correlation ID across Forum <-> sync-api requests
- [ ] Add metrics: success rate, error rate, latency, token exchange failures
- [ ] Add alerts for:
  - [ ] exchange-token failure spike
  - [ ] tool endpoint 5xx spike
  - [ ] p95 latency threshold breach
- [ ] Add timeout/retry policy for Forum -> sync-api calls
- [ ] Add fallback response strategy for provider failures
- [ ] Add audit logs for assistant tool invocations

**Exit criteria**
- [ ] Dashboards and alerts are active in staging
- [ ] Runbook exists for top 3 failure scenarios

---

## Phase 4 - Production Hardening (P0/P1)

- [ ] Replace minimal service auth with stronger approach (mTLS or signed service token)
- [ ] Finalize retention policy for assistant conversations and logs
- [ ] Add PII masking/redaction for prompt/result logs
- [ ] Add automated security scans in CI
- [ ] Add load test for peak expected assistant traffic
- [ ] Validate backup/restore for data stores

**Exit criteria**
- [ ] All production launch criteria in `gap-mvp-vs-production.md` are satisfied

---

## Cross-Cutting Test Checklist

- [ ] Unit tests for exchange-token services and validators
- [ ] Integration tests for token exchange and thread/message sync
- [ ] Integration tests for summarize/related/draft tools
- [ ] E2E test: login -> assistant -> summarize -> draft -> logout
- [ ] Negative tests: expired token, invalid signature, service timeout, provider error

---

## Launch Readiness Final Gate

- [ ] Security review signed off
- [ ] Platform/DevOps review signed off
- [ ] Product acceptance for core user journeys
- [ ] Rollback strategy tested
- [ ] Post-launch monitoring checklist completed
