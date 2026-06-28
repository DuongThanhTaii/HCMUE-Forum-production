# Forum x UEBot - Option A (Fast Integration)

## Goal

Turn UEBot into an AI Assistant module inside Forum quickly, without major backend refactoring.

- Add an AI Assistant route in Forum frontend.
- Reuse `UEBot/UE-Bot/web-app` (phase 1 can use iframe/micro-frontend).
- Reuse `UEBot/UE-Bot/sync-api` for threads/messages persistence.
- Add an SSO bridge from Forum token to sync-api token.

## Current System Context

Forum already has its own JWT auth system:
- `api/v1/auth/login`
- `api/v1/auth/refresh`

Forum already exposes post APIs and data:
- `api/v1/posts`
- `api/v1/search`
- Existing AI APIs:
  - `api/v1/ai/chat`
  - `api/v1/ai/summarize`
  - `api/v1/ai/search`

UEBot sync-api already includes:
- guest/otp/password/google authentication
- guest usage limits
- full thread/message CRUD

## Integration Architecture (Phase 1)

### Components

1. Forum Frontend
   - Add route: `/assistant` (or `/forum/assistant`)
   - Render UEBot UI in two ways:
     - A1: iframe to separately deployed UEBot web-app (fastest)
     - A2: mount UEBot module inside Forum frontend monorepo (better UX)

2. Sync API (separate service)
   - Run independently (separate port, e.g. `4010`)
   - Keep assistant chat data in sync-api Postgres

3. SSO Bridge (inside Forum backend)
   - Add token exchange endpoint to mint sync token from Forum user identity.

### Token Flow

1. User logs into Forum and receives Forum access token.
2. Frontend calls Forum bridge endpoint:
   - `POST /api/v1/integrations/uebot/exchange-token`
   - Header: `Authorization: Bearer <forum_token>`
3. Forum backend validates token and extracts `userId/email/name`.
4. Forum backend calls internal sync-api endpoint:
   - `POST /integrations/forum/exchange`
5. sync-api upserts user (by email) and issues sync JWT.
6. Frontend uses sync JWT for `threads/messages` requests to UEBot sync-api.

## Endpoints to Add Immediately

## 1) Forum backend (new)

- `POST /api/v1/integrations/uebot/exchange-token` (Authorize)
  - Input: empty body (or optional device info)
  - Output:
    - `syncAccessToken`
    - `syncExpiresAt`
    - `syncUser` { id, email, name }
    - `syncApiBaseUrl`

## 2) sync-api (new internal)

- `POST /integrations/forum/exchange`
  - Security: API key or mTLS
  - Input:
    - `externalUserId` (Forum user id)
    - `email`
    - `name`
    - `source`: `"forum"`
  - Behavior:
    - upsert `users` by email
    - map external id (add `external_user_id` column if needed)
    - issue sync-api JWT
  - Output:
    - `token`
    - `expiresAt`
    - `user`

## 3) Optional: context endpoint for AI tools

- `GET /api/v1/integrations/uebot/context`
  - Returns forum user metadata + roles + permissions
  - Used to prefill assistant context.

## Frontend Implementation Plan

### Phase 1 (iframe)

- Add `AssistantPage` in Forum frontend.
- The page loads UEBot web-app URL with parameters:
  - `syncApiUrl`
  - `syncToken` (prefer postMessage over query for security)
  - `theme`
  - `locale`
- Use postMessage to:
  - send new token after refresh
  - synchronize login/logout state

### Phase 1.5 (native embed)

- Extract reusable UI primitives from UEBot web-app.
- Reuse `sync-api.ts` client in Forum frontend.
- Remove iframe and keep a unified UX.

## Security Checklist

- Do not expose internal integration endpoint publicly.
- Add `FORUM_INTEGRATION_SHARED_SECRET` for sync-api.
- Rate-limit token exchange endpoint.
- Keep sync-api access token lifetime shorter than Forum token (e.g. 15-30 min).
- Avoid passing tokens in query string unless absolutely necessary.

## Data Ownership (Phase 1)

- Forum data remains in UniHub DB.
- Assistant conversation data remains in sync-api DB.
- Only light linking is required:
  - `forum_user_id` -> sync user
  - optional dedicated mapping table.

## Release Slices

- Slice 1: token exchange + `/assistant` route + iframe.
- Slice 2: login/logout sync events + token refresh.
- Slice 3: context tools (search post, summarize post) calling Forum API.

## Why Option A fits now

- Fastest time-to-market.
- Minimal impact to existing modules.
- Enables real adoption validation before deciding deeper merge (Option B).
