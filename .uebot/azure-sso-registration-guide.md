# Azure SSO Registration Guide (Forum + UEBot Integration)

This guide walks through a complete Azure Entra ID (Azure AD) setup for:
- **Forum API** as protected backend API
- **Forum Frontend** as public SPA client
- (Optional) **UEBot Sync API** if you later move its auth to Entra ID

The goal is to make your current dual-auth implementation ready for production migration.

---

## 1) Prerequisites

- An Azure subscription and an Entra ID tenant
- Admin or App Registration permissions
- Your environments and URLs:
  - Local API: `http://localhost:5034`
  - Local frontend: `http://localhost:5173` (or your Vite port)
  - Staging/Production domains
- Existing Forum local JWT path is still active (dual-auth mode)

---

## 2) Registration Strategy

Use **2 app registrations** first:

1. **UniHub Forum API** (Web/API)
   - Represents protected resources
   - Exposes scopes
2. **UniHub Forum Frontend** (Single-page application)
   - Requests access token for Forum API

Optional later:
3. **UEBot Sync API** (Web/API), only when migrating sync-api to Entra-issued tokens.

---

## 3) Register Forum API App

1. Go to **Azure Portal -> Microsoft Entra ID -> App registrations -> New registration**.
2. Name: `UniHub-Forum-API`.
3. Supported account types: pick your org policy (usually single tenant).
4. Redirect URI: leave empty for API app.
5. Create.

After creation, note:
- `Application (client) ID`
- `Directory (tenant) ID`

### 3.1 Expose API scope

1. Open app -> **Expose an API**.
2. Set **Application ID URI** (recommended):
   - `api://<forum-api-client-id>`
3. Add scope:
   - Scope name: `access_as_user`
   - Who can consent: `Admins and users` (or admin only, per policy)
   - Admin consent display name: `Access UniHub Forum API`
   - Admin consent description: `Allow app to call UniHub Forum API on behalf of user`
4. Save.

### 3.2 Token configuration (optional but recommended)

In **Token configuration**:
- Add optional claims if needed: `email`, `preferred_username`, `name`.
- Keep claims aligned with your backend claim extraction.

---

## 4) Register Forum Frontend SPA App

1. Create new app registration: `UniHub-Forum-Frontend`.
2. In **Authentication**:
   - Add platform: **Single-page application**
   - Redirect URIs:
     - Local: `http://localhost:5173` (or exact FE URL)
     - Prod: `https://<your-frontend-domain>`
3. Enable ID tokens / access tokens if required by your login library.

### 4.1 API permissions

1. Open **API permissions -> Add a permission -> My APIs**.
2. Select `UniHub-Forum-API`.
3. Choose delegated permission: `access_as_user`.
4. Click **Grant admin consent** (tenant admin action).

### 4.2 CORS/Origin checklist

Ensure frontend origin is allowed in your backend CORS configuration for each env.

---

## 5) Map Azure Values to Forum Backend Config

Set these values in `appsettings`/env:

- `Authentication__AzureAd__Enabled=true`
- `Authentication__AzureAd__TenantId=<tenant-id>`
- `Authentication__AzureAd__ClientId=<forum-api-client-id>`
- `Authentication__AzureAd__Audience=api://<forum-api-client-id>`
- `Authentication__AzureAd__Authority=https://login.microsoftonline.com/<tenant-id>/v2.0`
- `Authentication__AzureAd__ValidIssuer=https://login.microsoftonline.com/<tenant-id>/v2.0`

Important:
- `ClientId` here should match **Forum API app registration** (resource app).
- `Audience` must match your exposed API identifier.

---

## 6) Local Verification Flow

1. Start API with Azure enabled.
2. Login FE against Entra (or acquire token manually via Postman/MSAL sample).
3. Call protected endpoint with Azure bearer token:
   - `GET /auth/test`
4. Expect `200 OK`.
5. Also test legacy local JWT still works (dual-auth path).

If both pass, dynamic scheme routing is healthy.

---

## 7) UEBot Integration Considerations

Current architecture keeps:
- Forum token (local or Azure) -> Forum backend validates token
- Forum backend exchanges trusted identity with `sync-api` via shared secret
- `sync-api` still issues its own token

So Azure migration does **not** require immediate sync-api migration.

When ready for full Entra:
- register `UEBot Sync API` in Entra
- expose scope for sync operations
- replace shared-secret exchange with OBO or service-to-service token model

---

## 8) Production Hardening Checklist

- Restrict app registrations to expected tenant(s)
- Use separate app registrations per environment (dev/staging/prod)
- Lock redirect URIs exactly (no wildcards)
- Rotate secrets/certs; prefer certificates over client secrets for confidential clients
- Enforce Conditional Access + MFA per org policy
- Monitor sign-in logs and token failures in Entra
- Maintain rollback switch: `Authentication__AzureAd__Enabled=false`

---

## 9) Common Issues and Fixes

### `401 invalid_token` / issuer mismatch
- Check `Authority`, `ValidIssuer`, and tenant ID alignment.

### Audience mismatch
- Ensure token `aud` equals `api://<forum-api-client-id>` (or your configured API URI).

### Frontend gets token but API rejects
- Verify FE requested the correct scope from the **Forum API** app.
- Confirm admin consent was granted.

### Works locally but fails in production
- Missing production redirect URI or missing production origin in CORS.

---

## 10) Suggested Rollout

1. Dev tenant + dev environment end-to-end.
2. Staging with real org users and conditional access.
3. Enable Azure auth in production with dual-auth fallback still on.
4. Observe logs/metrics 1-2 weeks.
5. Decide Azure-first cutover window.

---

## 11) Quick Reference Variables

Forum API:
- `Authentication__AzureAd__Enabled`
- `Authentication__AzureAd__TenantId`
- `Authentication__AzureAd__ClientId`
- `Authentication__AzureAd__Audience`
- `Authentication__AzureAd__Authority`
- `Authentication__AzureAd__ValidIssuer`

Frontend (if using MSAL config):
- `VITE_AZURE_TENANT_ID`
- `VITE_AZURE_CLIENT_ID`
- `VITE_AZURE_API_SCOPE=api://<forum-api-client-id>/access_as_user`

---

If you want, next step can be a ready-to-copy **MSAL config template** for your frontend plus a small login button flow compatible with this registration.
