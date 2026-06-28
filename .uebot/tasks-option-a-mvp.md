# Tasks - Option A MVP (UEBot Assistant Module)

## Objective

Ship a working `/assistant` experience in Forum using UEBot web-app + sync-api with SSO token exchange.

## Milestone A1 - Backend Token Exchange

### Task A1.1 - Add Forum exchange endpoint
- **Target**: `POST /api/v1/integrations/uebot/exchange-token`
- **Suggested location**: new controller under `src/Modules/Identity/.../Controllers` or dedicated integrations controller.
- **Acceptance criteria**:
  - Requires Forum JWT (`[Authorize]`).
  - Returns `syncAccessToken`, `syncExpiresAt`, `syncUser`, `syncApiBaseUrl`.
  - Handles upstream sync-api errors gracefully.

### Task A1.2 - Implement sync-api internal exchange endpoint
- **Target**: `POST /integrations/forum/exchange`
- **File**: `UEBot/UE-Bot/sync-api/src/index.mjs`
- **Acceptance criteria**:
  - Protected by service auth (shared secret header at minimum).
  - Upserts user by stable identity.
  - Issues JWT compatible with existing sync-api auth middleware.

### Task A1.3 - Add integration configuration
- **Forum**:
  - Add `Integrations:UEBot:BaseUrl`
  - Add `Integrations:UEBot:SharedSecret`
- **sync-api**:
  - Add `FORUM_INTEGRATION_SHARED_SECRET`
- **Acceptance criteria**:
  - No hardcoded secrets in source.
  - Local and container env examples updated.

## Milestone A2 - Frontend Assistant Route

### Task A2.1 - Add assistant route/page in Forum frontend
- **Target route**: `/assistant`
- **Acceptance criteria**:
  - Route visible for authenticated users.
  - Renders UEBot app via iframe for phase 1.

### Task A2.2 - Secure token handoff
- **Method**: `postMessage` preferred over query string.
- **Acceptance criteria**:
  - Token refresh updates UEBot session.
  - Logout invalidates assistant session.
  - Allowed origin is explicitly validated.

### Task A2.3 - UI fallback states
- **Acceptance criteria**:
  - Loading and error states for exchange failure.
  - Retry action available.

## Milestone A3 - Complete UEBot Auth Hooks

### Task A3.1 - Implement `useAuth` missing functions
- **File**: `UEBot/UE-Bot/web-app/src/hooks/useAuth.ts`
- **Missing pieces**:
  - `requestOtp`
  - `verifyOtp`
  - `openGoogleLogin`
  - `completeOAuthFromUrl`
- **Acceptance criteria**:
  - Full login flow works with sync-api.
  - Guest-to-user state transition is correct.

### Task A3.2 - Wire AuthRequired dialog
- **File**: `UEBot/UE-Bot/web-app/src/containers/dialogs/AuthRequiredDialog.tsx`
- **Acceptance criteria**:
  - Shown when usage requires login.
  - Supports OTP/password/google path.

## Milestone A4 - Security and Ops Baseline

### Task A4.1 - Rate limit integration endpoints
- **Acceptance criteria**:
  - Exchange endpoint has stricter limits than normal API.

### Task A4.2 - Add observability
- **Acceptance criteria**:
  - Correlation id across Forum -> sync-api exchange.
  - Basic metrics: success/fail latency by endpoint.

### Task A4.3 - Smoke tests
- **Acceptance criteria**:
  - Login Forum -> open assistant -> create thread -> create message -> reload still synced.

## Definition of Done (Option A MVP)

- `/assistant` route works end-to-end for authenticated Forum users.
- Sync token exchange is secure and stable.
- Thread/message persistence works through sync-api.
- No critical security issue in integration path.
