# Azure SSO Demo Guide (Using `@onmicrosoft.com`)

This guide helps you demo Azure SSO quickly when your tenant has **not** verified `unihub.edu.vn` yet.

---

## 1) Goal

- Keep Forum local users in DB unchanged (`...@unihub.edu.vn`)
- Create Azure demo users using `@<tenant>.onmicrosoft.com`
- Log in with Azure accounts to prove Azure SSO path works

---

## 2) Find your tenant domain

In Azure Portal:

1. Go to `Microsoft Entra ID`
2. Open `Overview`
3. Note either:
   - `Primary domain`, or
   - `Tenant name` under `Custom domain names`

It usually looks like:

- `<your-tenant>.onmicrosoft.com`

Use that value in all usernames below.

---

## 3) Demo account mapping from Forum DB

Use this mapping so you can remember roles while presenting:

| Forum DB email | Azure demo UPN |
|---|---|
| `admin@unihub.edu.vn` | `admin@<your-tenant>.onmicrosoft.com` |
| `moderator@unihub.edu.vn` | `moderator@<your-tenant>.onmicrosoft.com` |
| `lecturer@unihub.edu.vn` | `lecturer@<your-tenant>.onmicrosoft.com` |
| `student@unihub.edu.vn` | `student@<your-tenant>.onmicrosoft.com` |
| `recruiter@unihub.edu.vn` | `recruiter@<your-tenant>.onmicrosoft.com` |

Optional extra users:

- `student2@<your-tenant>.onmicrosoft.com`
- `student3@<your-tenant>.onmicrosoft.com`
- `forum.test1@<your-tenant>.onmicrosoft.com`
- `bosch@<your-tenant>.onmicrosoft.com`
- `nab@<your-tenant>.onmicrosoft.com`
- `sap@<your-tenant>.onmicrosoft.com`

---

## 4) Create users in Entra ID

For each user:

1. `Microsoft Entra ID` -> `Users` -> `New user` -> `Create new user`
2. Fill:
   - `User principal name`: from mapping above (`@onmicrosoft.com`)
   - `Display name`: role-friendly name (e.g., `Student Demo`)
   - `Password`: set manually to `Admin@123456`
3. Disable forced password change (for smooth live demo)
4. Save

---

## 5) App/API permission checklist (must be ready)

For Azure SSO authentication path:

- Frontend SPA app has delegated permission to Forum API scope:
  - `api://<forum-api-client-id>/access_as_user`
- Admin consent is granted for that scope (if tenant policy requires)

For JIT guest invite (optional, can be OFF in personal tenant demo):

- `User.Invite.All` (Application)
- `User.Read.All` (Application)
- Admin consent granted

If you cannot grant admin consent, keep:

- `Authentication__AzureAd__EnableGuestInvitationOnLocalLogin=false`

---

## 6) Environment setup for this demo mode

In `src/UniHub.API/.env.api`:

```env
Authentication__AzureAd__Enabled=true
Authentication__AzureAd__EnableGuestInvitationOnLocalLogin=false
Authentication__AzureAd__TenantId=<tenant-id>
Authentication__AzureAd__ClientId=<forum-api-client-id>
Authentication__AzureAd__Audience=api://<forum-api-client-id>
Authentication__AzureAd__Authority=https://login.microsoftonline.com/<tenant-id>/v2.0
Authentication__AzureAd__ValidIssuer=https://login.microsoftonline.com/<tenant-id>/v2.0
```

In `frontend/.env`:

```env
VITE_AZURE_TENANT_ID=<tenant-id>
VITE_AZURE_CLIENT_ID=<frontend-spa-client-id>
VITE_AZURE_API_SCOPE=api://<forum-api-client-id>/access_as_user
```

Then restart API + frontend.

---

## 7) Live demo test flow

1. Open Forum frontend
2. Click Azure sign-in
3. Log in with e.g. `student@<your-tenant>.onmicrosoft.com` / `Admin@123456`
4. Verify:
   - Protected API endpoint works
   - Forum pages requiring auth load normally
   - SignalR chat/notifications negotiate successfully
5. Open assistant module route and verify existing Forum -> UEBot exchange flow still works

---

## 8) Suggested explanation during report

Use this line:

> We currently demonstrate Azure SSO using tenant-native `@onmicrosoft.com` identities.  
> Forum local identities remain in database. Automated local-to-Azure invitation is implemented but can be enabled later when tenant admin consent for Graph app permissions is available.

---

## 9) Rollback switch

If Azure demo fails right before presentation:

```env
Authentication__AzureAd__Enabled=false
```

Restart API and use local JWT login as backup.
