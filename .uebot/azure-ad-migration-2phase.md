# Azure AD Migration Path (Two-Phase)

## Goal

Move Forum + UEBot from current custom JWT flow to Azure AD with minimal downtime and low risk.

## Phase 1 - Dual Auth (Compatibility Mode)

### Objectives

- Keep current UniHub JWT fully working.
- Add Azure AD token validation in parallel.
- Keep existing UEBot token exchange for MVP continuity.

### Deliverables

- Dynamic auth scheme in backend:
  - Local JWT (`Bearer`)
  - Azure AD JWT (`AzureAd`)
- Configurable Azure options in appsettings/env:
  - `Authentication:AzureAd:*`
- API remains backward compatible with existing clients.

### Validation

- Local JWT requests still pass all protected endpoints.
- Azure AD tokens are accepted when `Authentication:AzureAd:Enabled=true`.
- No regression in refresh/login flows for existing app users.

## Phase 2 - Azure-First (Issuer Convergence)

### Objectives

- Make Azure AD the primary identity provider.
- Reduce dependency on local JWT issuance for client auth.
- Keep local token exchange only for internal service bridging where required.

### Deliverables

- Frontend login via Azure AD (Auth Code + PKCE).
- Role/permission mapping from Azure claims to app authorization model.
- Stable identity mapping by Azure `oid` (not email).
- Optional deprecation plan for legacy local login endpoints.

### Validation

- New sign-in path uses Azure AD end-to-end.
- Existing users mapped without account duplication.
- Protected routes and role gates work with Azure claims.

## Cutover Strategy

1. Enable Azure in staging, keep dual-auth.
2. Run shadow traffic tests with Azure tokens.
3. Gradually migrate selected user cohorts.
4. Finalize Azure-first once error rate and auth metrics are stable.

## Rollback Plan

- Toggle `Authentication:AzureAd:Enabled=false`.
- Keep local JWT as immediate fallback.
- Preserve token exchange compatibility for assistant flow.
