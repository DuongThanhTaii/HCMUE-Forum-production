# Azure SSO Demo Guide (Personal Account / Non-Admin School Tenant)

This guide is for the case where your school tenant restricts permissions (cannot create users, cannot grant admin consent).

Goal:
- Use a personal account setup to demo Azure SSO end-to-end.
- Keep costs at zero or near-zero.

---

## 1) Which account setup should you use?

Use one of these:

1. **Microsoft 365 Developer Program sandbox** (recommended for demos)
2. A personal tenant where you are the tenant admin

Do **not** rely on the school-managed tenant if your role has no admin rights.

---

## 2) Cost expectation

For SSO demo scope (app registration, users, token flow), this is typically free.

Avoid creating billable Azure resources (App Service, Azure SQL, etc.) unless needed.

---

## 3) Create or access your own tenant

If using Microsoft 365 Developer Program:

1. Sign in with your personal Microsoft account.
2. Create a developer sandbox tenant.
3. Note your new tenant domain, e.g. `yourtenant.onmicrosoft.com`.

Then switch Azure Portal directory to that tenant.

---

## 4) Create full demo users (mapped from Forum DB)

In `Microsoft Entra ID -> Users -> New user -> Create new user`:

Create all users below (full set from Forum database):

| Forum email | Role | Azure UPN to create |
|---|---|---|
| `admin@unihub.edu.vn` | Admin | `admin@<yourtenant>.onmicrosoft.com` |
| `moderator@unihub.edu.vn` | Moderator | `moderator@<yourtenant>.onmicrosoft.com` |
| `lecturer@unihub.edu.vn` | Lecturer | `lecturer@<yourtenant>.onmicrosoft.com` |
| `student@unihub.edu.vn` | Student | `student@<yourtenant>.onmicrosoft.com` |
| `student2@unihub.edu.vn` | Student | `student2@<yourtenant>.onmicrosoft.com` |
| `student3@unihub.edu.vn` | Student | `student3@<yourtenant>.onmicrosoft.com` |
| `bosch@unihub.edu.vn` | Recruiter | `bosch@<yourtenant>.onmicrosoft.com` |
| `nab@unihub.edu.vn` | Recruiter | `nab@<yourtenant>.onmicrosoft.com` |
| `sap@unihub.edu.vn` | Recruiter | `sap@<yourtenant>.onmicrosoft.com` |

Suggested password for all demo users:

- `Admin@123456`

For each created user:

- Set the password manually to `Admin@123456`.
- Disable forced password change at first sign-in for smooth demo.
- Keep a simple mapping note (Forum email -> Azure UPN) for presentation.

---

## 5) Register two apps

### 5.1 Backend API app

Create app registration: `UniHub-Forum-API`

- Save:
  - Tenant ID
  - Application (client) ID

In **Expose an API**:
- Set Application ID URI: `api://<forum-api-client-id>`
- Add scope: `access_as_user`

### 5.2 Frontend SPA app

Create app registration: `UniHub-Forum-Frontend`

In **Authentication**:
- Add SPA redirect URIs:
  - `http://localhost:5173`

In **API permissions**:
- Add delegated permission from your API:
  - `access_as_user`
- Grant admin consent (in your own tenant, you can do this).

---

## 6) (Optional) JIT invite permissions

Only needed if you want backend auto-invite local users to Azure.

For backend API app -> **API permissions** -> Microsoft Graph -> Application:
- `User.Invite.All`
- `User.Read.All`

Then **Grant admin consent**.

If you skip this, set:
- `Authentication__AzureAd__EnableGuestInvitationOnLocalLogin=false`

---

## 7) Configure project env

### 7.1 `src/UniHub.API/.env.api`

```env
Authentication__AzureAd__Enabled=true
Authentication__AzureAd__TenantId=<tenant-id>
Authentication__AzureAd__ClientId=<forum-api-client-id>
Authentication__AzureAd__ClientSecret=<backend-client-secret-if-needed>
Authentication__AzureAd__Audience=api://<forum-api-client-id>
Authentication__AzureAd__Authority=https://login.microsoftonline.com/<tenant-id>/v2.0
Authentication__AzureAd__ValidIssuer=https://login.microsoftonline.com/<tenant-id>/v2.0

Authentication__AzureAd__EnableGuestInvitationOnLocalLogin=false
Authentication__AzureAd__InvitationRedirectUrl=http://localhost:5173
Authentication__AzureAd__GraphBaseUrl=https://graph.microsoft.com
Authentication__AzureAd__InvitationSendMail=true
```

### 7.2 `frontend/.env`

```env
VITE_AZURE_TENANT_ID=<tenant-id>
VITE_AZURE_CLIENT_ID=<frontend-spa-client-id>
VITE_AZURE_API_SCOPE=api://<forum-api-client-id>/access_as_user
```

---

## 8) Restart and test

1. Restart API
2. Restart frontend
3. Login with `student@<yourtenant>.onmicrosoft.com`
4. Verify:
   - protected API works (`/auth/test`)
   - SignalR negotiate works
   - Forum normal auth-required pages work

If using UEBot integration, verify assistant route still exchanges token successfully.

---

## 9) Demo script (short)

1. Show Azure login with student demo account.
2. Show protected page/API success.
3. Show chat/notification realtime connection.
4. Show assistant route still works with SSO chain.
5. Mention fallback: local JWT can be toggled if Azure disabled.

---

## 10) Common issues

### `401` after Azure login
- Check `aud` in token equals `api://<forum-api-client-id>`
- Check `Authority` / `ValidIssuer` tenant ID

### No admin consent button
- You are likely in wrong directory or wrong role.
- Switch to personal tenant where you are admin.

### JIT invite fails
- Missing Graph app permissions or consent.
- Keep `EnableGuestInvitationOnLocalLogin=false` for demo.
