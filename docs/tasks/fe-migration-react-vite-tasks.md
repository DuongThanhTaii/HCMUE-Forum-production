# FE Migration Tasks — React + Vite + React Router
**Scope:** Migrate from Next.js frontend to React + Vite while preserving UI/system design.

---

## FE-MIG-01 — Bootstrap Vite workspace
- Create `frontend-react` via Vite React TS template
- Setup eslint, prettier, tsconfig paths
- Acceptance: `npm run dev` works

## FE-MIG-02 — Tailwind v4 + Shadcn base
- Install Tailwind v4 and Shadcn dependencies
- Port HCMUE tokens and typography
- Acceptance: button/card/theme parity with current design

## FE-MIG-03 — Shared infrastructure
- Port `shared/lib/api/client`, utils, env typing, query provider
- Port Sonner and global app shell
- Acceptance: API call works against backend

## FE-MIG-04 — Router skeleton
- Configure React Router with route groups:
  - auth/main/mod/admin
- Create placeholders for all design-spec routes
- Acceptance: all routes resolve without 404

## FE-MIG-05 — i18n migration
- Integrate `react-i18next`
- Reuse `vi.json` and `en.json`
- Replace translation hooks in shell components
- Acceptance: language switch works

## FE-MIG-06 — Auth and guards
- Port auth store/hooks
- Implement `RequireAuth`, `RequireModeratorOrAdmin`, `RequireAdmin`
- Acceptance: protected routes correctly redirect

## FE-MIG-07 — Main/mod/admin layouts
- Build skeleton layouts with sidebar/topbar
- Apply Tailwind Tree comments
- Acceptance: structure matches design doc

## FE-MIG-08 — Auth slice
- Login/register/forgot/reset pages
- Validation with RHF + Zod
- Acceptance: API integration success

## FE-MIG-09 — Forum slice
- List/detail/create/edit, comments, vote, bookmark, report
- Acceptance: forum E2E happy path works

## FE-MIG-10 — Learning slice
- Documents list/detail/upload/rate/summary
- Courses/faculties views
- Acceptance: upload + detail + rate flow works

## FE-MIG-11 — Career slice
- Jobs list/detail/saved/applications/company profile
- Acceptance: submit application flow works

## FE-MIG-12 — Chat slice
- Conversations/messages/channels/reactions/attachments
- SignalR connection and receive events
- Acceptance: real-time send/receive works

## FE-MIG-13 — Notification slice
- Bell dropdown + full page + preferences
- SignalR push updates
- Acceptance: unread badge real-time updates

## FE-MIG-14 — Profile slice
- Profile view + settings update
- Acceptance: update profile API flow works

## FE-MIG-15 — Moderation slice
- Reports, post actions, learning approvals, AI queue
- Acceptance: moderator actions functional

## FE-MIG-16 — Admin slice
- Users/roles/permissions/endpoints/logs/career moderation
- Acceptance: admin tables and actions functional

## FE-MIG-17 — Quality gates
- Typecheck, lint, unit tests, smoke tests
- Fix route/import regressions
- Acceptance: CI green

## FE-MIG-18 — Build and deploy pipeline
- Configure Vite production build and hosting pipeline
- Environment variable mapping
- Acceptance: preview + production deploy successful

## FE-MIG-19 — Cutover and cleanup
- Compare parity checklist
- Switch main frontend target
- Archive legacy Next frontend
- Acceptance: signed-off release checklist

---

## Suggested Execution Order
`01 -> 02 -> 03 -> 04 -> 05 -> 06 -> 07 -> (08..16 in waves) -> 17 -> 18 -> 19`

