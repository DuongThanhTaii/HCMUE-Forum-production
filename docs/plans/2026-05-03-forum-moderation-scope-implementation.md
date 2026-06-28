# Implementation plan: Forum moderation scope (handoff for agent)

**Date:** 2026-05-03  
**Priority:** High (security / consistency with draft queue)  
**Design:** `docs/specs/2026-05-03-forum-moderation-scope-design.md`

---

## Objective

1. **`GET /api/v1/mod/reports`** — Moderators only see reports for content in categories they moderate; Admins see all.
2. **`POST /api/v1/mod/reports/{id}/resolve`** — Same rule: Moderators cannot resolve out-of-scope reports (prefer **403** with clear message).

---

## Phase 1 — Backend: scoped report listing

### 1.1 Extend report query

- [ ] Add optional parameter to `GetReportsQuery` / handler, e.g. `IReadOnlyList<Guid>? CategoryScope` where:
  - `null` = no filter (Admin).
  - Empty = no results for Moderator with no assignments.
  - Non-empty = filter reports by effective category (see design doc).

### 1.2 Extend repository

- [ ] Update `IReportRepository.GetReportsAsync` signature to accept `CategoryScope`.
- [ ] Implement in `ReportRepository` using EF joins:
  - Post-type reports → `Posts.CategoryId`
  - Comment-type reports → `Comments` join `Posts` → `CategoryId`
- [ ] Add/adjust tests if the solution has test projects for Forum; otherwise verify with manual API calls.

### 1.3 Wire `ModerationController.GetReports`

- [ ] Reuse the **same** category list computation as `GetPendingPosts` (lines ~149–158 in `ModerationController.cs`): load categories, filter by `ModeratorIds` for non-Admin.
- [ ] Pass computed scope into `GetReportsQuery`.
- [ ] Ensure **Admin** path passes `null` scope (full list).

### 1.4 Verification (backend)

- [ ] As **Admin**, list reports — expect unchanged total (minus any join bugs).
- [ ] As **Moderator** with assignment to **category A only**, create reports on posts in **B** — expect **not listed**.
- [ ] As **Moderator** with **no** category assignments — expect **empty** list.

---

## Phase 2 — Backend: authorize resolve

### 2.1 Single place for scope resolution

- [ ] Extract shared helper or service, e.g. `IModerationCategoryScope` / `ModerationScopeService`:
  - `Task<IReadOnlyList<Guid>?> GetCategoryScopeForUserAsync(Guid userId, bool isAdmin, CancellationToken)`
  - Uses `ICategoryRepository.GetAllAsync` + `ModeratorIds` (same as controller today).

### 2.2 Effective category for a `Report`

- [ ] Add method like `Task<Guid?> GetEffectiveCategoryIdAsync(Report report, CancellationToken)`:
  - Post → post’s `CategoryId`
  - Comment → load comment → post’s `CategoryId`
  - Missing entities → return null (treat as **cannot** moderate for Moderator).

### 2.3 `ResolveModerationReportCommandHandler`

- [ ] After loading the report, determine **IsAdmin** for `request.ReviewerId` (via `IUser`/`UserManager`/role check — follow existing patterns in the API host).
- [ ] If **not** Admin: compute scope; if effective category not in scope → **failure** / forbid.
- [ ] Map failure to **403** in controller if using authorization-style errors, or return ProblemDetails.

### 2.4 Verification (backend)

- [ ] Moderator **A** (category 1) attempts resolve on report for category 2 → **403** or equivalent.
- [ ] Admin resolves same → **200**.
- [ ] In-scope Moderator resolves → **200**, behavior unchanged for keep/remove.

---

## Phase 3 — Frontend (optional polish, after Phase 1–2)

Only if time permits; **scope fix is valid with no FE changes**.

- [ ] `ModReportsPage` / `useModReportsPage.ts`: show **human-readable** `ReportReason` (map enum to i18n).
- [ ] Add links: “Open post” / “Open comment context” using `reportedItemId` + `reportedItemType` (build URLs like `/forum/{postId}#comment-{id}` when applicable).
- [ ] Handle **403** from resolve with a toast instead of generic error.
- [ ] Pagination: API already supports `pageNumber` / `pageSize` — wire UI if only page 1 is used today.

Files:

- `frontend/src/features/forum/hooks/useModReportsPage.ts`
- `frontend/src/features/forum/components/ModReportsPage.tsx`
- `frontend/src/features/forum/api/forum.moderation.api.ts`

---

## Phase 4 — Documentation hygiene

- [ ] Update `docs/tasks/fe-14-mod-reports.md` endpoints to match production routes (`/api/v1/mod/reports`, etc.) and mark scope work done.
- [ ] Optionally add a short subsection to `docs/api/forum-fe-api.md` under moderation routes documenting **Moderator scope** behavior.

---

## Rollback / risk

- **Risk:** EF join incorrect → missing or duplicate reports. Mitigate with SQL logging or staging tests.
- **Risk:** Role detection wrong → Moderators blocked entirely. Mitigate by comparing `User.IsInRole("Admin")` with the same claims used elsewhere.

---

## Definition of done

- [ ] Moderators cannot list or resolve reports outside assigned categories.
- [ ] Admins retain full access.
- [ ] No regression on public report submission (`POST .../report`).
- [ ] Design doc `docs/specs/2026-05-03-forum-moderation-scope-design.md` remains accurate or is updated in the same PR.

---

## Suggested branch name

`feature/forum-mod-report-scope`
