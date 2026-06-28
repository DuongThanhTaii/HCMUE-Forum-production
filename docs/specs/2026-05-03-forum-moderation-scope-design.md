# Forum moderation scope — design (scoped reports & resolve auth)

**Date:** 2026-05-03  
**Goal:** Align **report queues** and **resolve actions** with the same **category-based scope** already used for **pending draft posts** (`GET /api/v1/mod/posts`).  
**Audience:** Implementing agent (backend + optional frontend).

---

## Problem statement

- **`GET /api/v1/mod/posts`** already restricts **Moderators** to draft posts whose **post category** is one where the user appears in `Category.ModeratorIds`. **Admins** see all categories.
- **`GET /api/v1/mod/reports`** currently lists **all reports** with no category filter — Moderators can see and act on reports outside their assignment.
- **`ResolveModerationReport`** must not allow a Moderator to resolve (keep/remove) a report for content outside their scope.

---

## Domain mapping: “khoa” vs category

- **Operational mapping (current codebase):** Forum **`Category`** carries **`ModeratorIds`**. Treat **category as the moderation boundary** (“khoa” ≈ forum category until a dedicated `FacultyId` exists on posts/categories).
- **Future:** If product adds explicit faculty fields, extend scope filters to include `FacultyId`; keep the same pattern (scoped query + authorize resolve).

---

## Resolving “effective category” for a report

| `ReportedItemType` | How to get category |
|--------------------|---------------------|
| **Post**           | `Post.CategoryId` where `Post.Id == Report.ReportedItemId` |
| **Comment**        | `Comment.PostId` → `Post.CategoryId` where `Comment.Id == Report.ReportedItemId` |

Deleted/missing targets: existing snapshot logic in `ModerationController.GetSnapshotAsync` already surfaces `IsTargetDeleted`. Scope rules:

- If target is **missing** or **deleted**: recommend **Admin-only** resolve OR **403** for Moderator (implementation choice — document in PR; safest is Moderator **cannot** resolve until Admin reviews).

---

## Scope model for queries

Introduce a single notion used by both list and resolve:

- **`CategoryScope`**
  - **`null`** — no category filter (**Admin** viewing full tenant queue).
  - **Empty list** — Moderator assigned to **no** categories: **zero** visible reports / cannot resolve.
  - **Non-empty** — filter reports whose effective category is **in** this set.

Compute scope **once** in `ModerationController` (same pattern as `GetPendingPosts`):

```csharp
// Pseudocode — mirror existing GetPendingPosts logic
if (User.IsInRole("Admin")) scopedCategoryIds = null;
else {
    scopedCategoryIds = categories
        .Where(c => c.ModeratorIds.Contains(moderatorId))
        .Select(c => c.Id.Value)
        .ToList();
}
```

---

## Repository / EF considerations

- Extend `IReportRepository.GetReportsAsync` (or add an overload) to accept optional **`IReadOnlyList<Guid>? categoryIds`**.
- Implement filtering with **EF Core joins** (avoid loading all reports into memory):
  - **Post reports:** join `Reports` → `Posts` on `ReportedItemId` / `Post.Id`, filter `Post.CategoryId`.
  - **Comment reports:** join `Reports` → `Comments` → `Posts` on comment id and `Comment.PostId == Post.Id`, filter `Post.CategoryId`.
- When `categoryIds` is **null**, preserve current behavior (no join filter).

---

## Authorization for resolve

- **`ResolveModerationReportCommandHandler`** must validate the reviewer before mutating:
  - **Admin:** always allowed (subject to existing domain rules: report exists, not already resolved).
  - **Moderator:** allowed only if effective category of the report’s target is in their scoped category list.
- Prefer a small dedicated collaborator (e.g. `IModerationReportAccess` or `IModerationScopeService`) to avoid duplicating scope logic between controller and handler.
- Return **403** or a domain **`Result.Failure`** with a stable error code (e.g. `Moderation.Forbidden`) for out-of-scope resolve attempts.

---

## API contract (backward compatible)

- **No URL changes** required if scope is applied server-side from `User` roles/claims.
- Optional later: query params `categoryId` for **extra** UI filtering (Admin); not required for MVP scope fix.

---

## Related files (starting points)

| Layer | File |
|-------|------|
| API | `UniHub.Forum.Presentation/Controllers/ModerationController.cs` |
| Query | `UniHub.Forum.Application/Queries/GetReports/*` |
| Command | `UniHub.Forum.Application/Commands/ResolveModerationReport/*` |
| Repo | `UniHub.Forum.Infrastructure/Persistence/Repositories/ReportRepository.cs` |
| Abstractions | `UniHub.Forum.Application/Abstractions/IReportRepository.cs` |
| Domain | `UniHub.Forum.Domain/Comments/Comment.cs` (`PostId`), `Posts` for `CategoryId` |

---

## Out of scope for this design

- FE-14 task doc still mentions **dismiss** and wrong API paths — update tasks separately after BE stabilizes.
- **Learning** document approvals (FE-15) — different module.
- **Realtime** notifications to moderators — optional follow-up.

---

## References

- Existing task sketches: `docs/tasks/fe-14-mod-reports.md`, `docs/tasks/fe-04-mod-layout.md`
- Forum FE API: `docs/api/forum-fe-api.md`
