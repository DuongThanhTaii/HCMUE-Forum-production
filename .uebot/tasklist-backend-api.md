# Backend Tasklist (Forum + sync-api)

## A. Forum Backend - Integration Layer

### A1. Contracts and DTOs
- [ ] Create request/response DTOs for exchange-token endpoint
- [ ] Add schema examples to API docs/comments
- [ ] Add validation (empty body allowed or explicit fields)

### A2. Exchange endpoint
- [ ] Implement `POST /api/v1/integrations/uebot/exchange-token`
- [ ] Require `[Authorize]`
- [ ] Resolve user identity from JWT claims (not from client body/query)
- [ ] Call sync-api internal exchange client
- [ ] Return normalized payload to frontend

### A3. HTTP client and resilience
- [ ] Add typed client for sync-api integration
- [ ] Add timeout policy
- [ ] Add retry policy with bounded retries
- [ ] Add circuit breaker (if policy library already in stack)

### A4. Security
- [ ] Add secure outbound credential (shared secret header initially)
- [ ] Add endpoint rate limit profile
- [ ] Add structured security logs for denied/failed exchanges

---

## B. sync-api - Trusted Exchange and Identity Mapping

### B1. Internal exchange endpoint
- [ ] Add `POST /integrations/forum/exchange`
- [ ] Verify service credential before processing
- [ ] Reject unknown/invalid source

### B2. Identity mapping
- [ ] Define external identity mapping strategy:
  - [ ] add `external_user_id` in `users`, or
  - [ ] create separate mapping table
- [ ] Make mapping deterministic and idempotent
- [ ] Ensure no duplicate user creation on retries

### B3. Token issuance
- [ ] Issue sync JWT with stable claims
- [ ] Define token expiry for integration flow
- [ ] Ensure compatibility with existing `authMiddleware`

---

## C. Forum Co-Pilot Tools API

### C1. Tools controller
- [ ] Add `/api/v1/assistant/tools` controller
- [ ] Add consistent error and response model
- [ ] Add authorization policy and rate limiting

### C2. Summarize tool
- [ ] Implement `summarize-post`
- [ ] Fetch post + comments from Forum data layer
- [ ] Call summarization service
- [ ] Return summary + key points + citations (if available)

### C3. Related tool
- [ ] Implement `related-posts`
- [ ] Use real search data path (remove mock in prod path)
- [ ] Return ranked results with reasoning field

### C4. Draft tool
- [ ] Implement `draft-reply`
- [ ] Add tone/intent handling
- [ ] Return draft only (no auto-post behavior)

---

## D. Observability and Audit

- [ ] Add correlation IDs for exchange and tools calls
- [ ] Add metrics counters/timers per endpoint
- [ ] Add audit logs:
  - [ ] actor id
  - [ ] tool name
  - [ ] request timestamp
  - [ ] outcome status

---

## E. Backend Definition of Done

- [ ] Endpoints implemented and documented
- [ ] Integration and negative-path tests pass
- [ ] No P0 security finding open
- [ ] Staging smoke tests pass
