# Forum x UEBot Readiness Check

## Scope

This check reviews integration readiness for:
- Option A: UEBot as an AI Assistant module in Forum.
- Option C: UEBot as a Forum Co-Pilot.

Projects reviewed:
- `E:/HCMUE-Forum` (Forum backend/frontend)
- `E:/HCMUE-Forum/UEBot/UE-Bot` (UEBot web-app + sync-api)

## Executive Verdict

Overall readiness: **Medium (about 60%)**.

- Core building blocks exist (JWT auth, Forum APIs, AI controllers, UEBot sync endpoints).
- MVP is feasible quickly.
- Not production-ready yet due to security and implementation gaps.

## What Is Already Good

### Forum project

- Modular backend structure is in place (`Identity`, `Forum`, `AI`, etc.).
- JWT login/refresh exists in `src/Modules/Identity/UniHub.Identity.Presentation/Controllers/AuthController.cs`.
- Core Forum APIs already available (`/api/v1/posts`, `/api/v1/search`).
- AI endpoints already available (`/api/v1/ai/chat`, `/api/v1/ai/summarize`, `/api/v1/ai/search`).

### UEBot project

- `sync-api` has:
  - auth flows (guest, otp, password, google)
  - usage limits for guest
  - thread/message CRUD
  - auto migrations
  - source: `UEBot/UE-Bot/sync-api/src/index.mjs`
- `web-app` has sync client and service switch logic:
  - `UEBot/UE-Bot/web-app/src/lib/sync-api.ts`
  - `UEBot/UE-Bot/web-app/src/services/threads/default.ts`
  - `UEBot/UE-Bot/web-app/src/services/messages/default.ts`

## Critical Gaps Before MVP

1. Missing SSO bridge contract (Option A core):
   - No Forum endpoint for exchange token.
   - No sync-api internal endpoint for trusted forum exchange.

2. UEBot web-app auth flow is incomplete:
   - `useAuth` contains stubbed methods in:
     - `UEBot/UE-Bot/web-app/src/hooks/useAuth.ts`

3. Security hardening needed:
   - Internal exchange must use service auth (shared secret or mTLS).
   - Token exchange endpoint should be rate-limited.
   - Avoid exposing sync tokens via query string.

4. Co-Pilot tool layer is missing:
   - No `/api/v1/assistant/tools/*` proxy endpoints in Forum yet.

## Risks

- Identity mismatch risk if sync user mapping is not deterministic.
- Data leakage risk if internal integration endpoints are publicly reachable.
- UX risk if iframe token lifecycle is not handled via secure postMessage flow.
- Quality risk for Option C without retrieval/citation guardrails.

## Recommended Go/No-Go

- **Go for MVP** only after:
  - token exchange endpoints implemented
  - auth stubs completed in UEBot web-app
  - minimal security controls in place
- **No-Go for public production** until:
  - logging/audit + stronger guardrails + persistence quality checks are complete

## Immediate Next Step

Execute tasks in:
- `.uebot/tasks-option-a-mvp.md`
- `.uebot/tasks-option-c-copilot-mvp.md`
- `.uebot/tasks-integration-master-plan.md`
