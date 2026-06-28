# FE-09: Career Hub

| Property | Value |
|---|---|
| **ID** | FE-09 |
| **Branch** | `feature/FE-09-career` |
| **Commit** | `feat(fe/career): implement job listings, company profiles and applications` |
| **Priority** | High |
| **Estimate** | 10h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-03 |

---

## API Endpoints

| Action | Method | Endpoint |
|---|---|---|
| List jobs | GET | `/api/v1/career/jobs?page=&search=&type=&location=` |
| Search jobs | GET | `/api/v1/career/jobs/search?q=` |
| Get job | GET | `/api/v1/career/jobs/{id}` |
| Matching jobs | GET | `/api/v1/career/jobs/matching` |
| Save job | POST | `/api/v1/career/jobs/{id}/save` |
| Unsave job | DELETE | `/api/v1/career/jobs/{id}/save` |
| Saved jobs | GET | `/api/v1/career/jobs/saved` |
| Is saved | GET | `/api/v1/career/jobs/{id}/saved` |
| Submit application | POST | `/api/v1/career/jobs/{id}/apply` |
| Withdraw application | DELETE | `/api/v1/career/applications/{id}` |
| My applications | GET | `/api/v1/career/applications/me` |
| Get application | GET | `/api/v1/career/applications/{id}` |
| Get company | GET | `/api/v1/career/companies/{id}` |
| Register company | POST | `/api/v1/career/companies` |

---

## Pages

### `/career/jobs` — Job List

**Filter bar:**
```
Search: [___________] Type: [Toàn thời gian ▼] [Tìm kiếm]
```

**Job Card:**
```
┌─────────────────────────────────────────────┐
│ [Logo] Company Name     [🔖 Save]           │
│ Tên vị trí (font-heading)                   │
│ 📍 TP.HCM  ⏰ Full-time  💰 12-20M         │
│ 🤖 Match: 87%  [Còn 5 ngày]                │
│                                             │
│ Tags: [React] [TypeScript]    [Apply →]     │
└─────────────────────────────────────────────┘
```

- AI Match badge: từ `GET /api/v1/career/jobs/matching` → map score lên từng job card
- Deadline badge: `bg-accent/10 text-accent` nếu < 3 ngày
- Save/Unsave: optimistic update

### `/career/jobs/[id]` — Job Detail

```
┌───────────────────────────────┬──────────────────────┐
│  JOB DESCRIPTION              │  APPLY PANEL         │
│                               │  [Company Logo]      │
│  Title + Company              │  Company Name        │
│  Location / Type / Salary     │  [🔖 Save Job]       │
│  Deadline: dd/mm/yyyy         │  [Apply Now]         │
│                               │                      │
│  Mô tả công việc...           │  AI Match: 87%       │
│  Yêu cầu...                   │  [View Company →]    │
│  Quyền lợi...                 │                      │
└───────────────────────────────┴──────────────────────┘
```

Apply button → modal: cover letter textarea + submit.

### `/career/jobs/saved` — Saved Jobs

Grid of saved job cards. Unsave button on each.

### `/career/applications` — My Applications

Table/list:
| Job | Company | Applied | Status | Action |
|-----|---------|---------|--------|--------|
| ... | ... | 28/4 | Pending | [Withdraw] |
| ... | ... | 26/4 | Accepted | ✓ |
| ... | ... | 20/4 | Rejected | — |

Status badge colors:
- Pending: yellow
- Accepted: success green
- Rejected: destructive red
- Withdrawn: muted

### `/career/companies/[id]` — Company Profile

Logo + name + description + open jobs list.

---

## Components to Create

```
components/features/career/
├── JobCard.tsx
├── JobList.tsx             ← with filters
├── JobDetail.tsx
├── ApplyModal.tsx          ← cover letter form
├── SaveJobButton.tsx       ← optimistic toggle
├── ApplicationCard.tsx
├── ApplicationList.tsx
└── CompanyProfile.tsx
```

---

## Acceptance Criteria

- [ ] Job list với search + type filter
- [ ] AI Match score hiện trên job cards
- [ ] Save/Unsave với optimistic update
- [ ] Apply modal submit được
- [ ] My applications hiện đúng status
- [ ] Withdraw application hoạt động
- [ ] Deadline countdown badge
- [ ] Company profile page load
