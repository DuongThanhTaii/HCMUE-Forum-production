# FE-08: Learning Hub

| Property | Value |
|---|---|
| **ID** | FE-08 |
| **Branch** | `feature/FE-08-learning` |
| **Commit** | `feat(fe/learning): implement documents, courses, faculties and upload` |
| **Priority** | High |
| **Estimate** | 12h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-03 |

---

## API Endpoints

| Action | Method | Endpoint |
|---|---|---|
| Search documents | GET | `/api/v1/learning/documents/search?q=&faculty=&course=&page=` |
| Get document | GET | `/api/v1/learning/documents/{id}` |
| Upload document | POST | `/api/v1/learning/documents` (multipart) |
| Download document | POST | `/api/v1/learning/documents/{id}/download` |
| Rate document | POST | `/api/v1/learning/documents/{id}/rate` body: `{ score: 1-5 }` |
| Get courses | GET | `/api/v1/learning/courses` |
| Get course | GET | `/api/v1/learning/courses/{id}` |
| Get faculties | GET | `/api/v1/learning/faculties` |
| Approval queue | GET | `/api/v1/learning/documents?status=Pending` (Mod only) |
| Approve doc | POST | `/api/v1/learning/documents/{id}/approve` |
| Reject doc | POST | `/api/v1/learning/documents/{id}/reject` |
| Request revision | POST | `/api/v1/learning/documents/{id}/request-revision` |
| Summarize | POST | `/api/v1/ai/summarize` body: `{ documentId }` |

---

## Tailwind Layout Tree

### DocumentCard tree

```
DocumentCard [group flex flex-col rounded-xl border border-border bg-card shadow-card hover:shadow-card-hover transition-shadow duration-200 cursor-pointer overflow-hidden]
├── CardTop [flex items-start gap-3 p-4 pb-3]
│   ├── FileIconBox [w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0]
│   │   ├── [pdf]  [bg-red-100 text-red-600 dark:bg-red-900/20 dark:text-red-400]
│   │   ├── [docx] [bg-blue-100 text-blue-600 dark:bg-blue-900/20 dark:text-blue-400]
│   │   └── [pptx] [bg-orange-100 text-orange-600 dark:bg-orange-900/20 dark:text-orange-400]
│   ├── DocMeta [flex-1 min-w-0]
│   │   ├── DocTitle [text-sm font-heading font-semibold text-foreground line-clamp-2 mb-0.5 group-hover:text-primary transition-colors]
│   │   ├── CourseName [text-xs text-muted truncate]
│   │   └── FacultyName [text-xs text-muted/70 truncate]
│   └── StatusBadge [px-2 py-0.5 rounded-full text-[11px] font-medium shrink-0]
│       ├── [Approved]  [bg-success/10 text-success]
│       ├── [Pending]   [bg-yellow-100 text-yellow-700 dark:bg-yellow-900/20 dark:text-yellow-400]
│       └── [Rejected]  [bg-destructive/10 text-destructive]
└── CardFooter [flex items-center justify-between px-4 py-2.5 border-t border-border/60 bg-muted/20]
    ├── FooterLeft [flex items-center gap-3 text-xs text-muted]
    │   ├── Rating [flex items-center gap-1]
    │   │   ├── StarIcon [w-3 h-3 text-yellow-500]
    │   │   └── Score [tabular-nums]
    │   └── Downloads [flex items-center gap-1]
    │       ├── DownloadIcon [w-3 h-3]
    │       └── Count [tabular-nums]
    └── DownloadButton [w-7 h-7 rounded-lg flex items-center justify-center bg-primary/10 text-primary hover:bg-primary hover:text-primary-foreground transition-colors cursor-pointer]
        └── DownloadIcon [w-3.5 h-3.5]
```

### Document Detail tree

```
DocumentDetailPage [flex gap-6]
├── DocumentViewer [flex-1 min-w-0 rounded-xl border border-border bg-card overflow-hidden]
│   ├── ViewerHeader [flex items-center justify-between px-4 py-3 border-b border-border]
│   │   ├── FileName [text-sm font-medium text-foreground truncate]
│   │   └── ViewerActions [flex items-center gap-2]
│   │       ├── DownloadBtn [flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-primary text-primary-foreground text-xs font-medium hover:bg-primary-hover cursor-pointer transition-colors]
│   │       └── ReportBtn [text-xs text-muted hover:text-destructive cursor-pointer]
│   └── ViewerContent [min-h-[500px] bg-muted/20]
│       └── PDFEmbed / FilePreview [w-full h-full]
└── SidePanel [w-72 shrink-0 space-y-4]
    ├── RatingCard [rounded-xl border border-border bg-card p-4]
    │   ├── CardTitle [text-sm font-heading font-semibold mb-3]
    │   ├── StarRating [flex items-center gap-1 mb-3]
    │   │   └── Star (×5) [w-6 h-6 cursor-pointer transition-colors]
    │   │       ├── [filled]  [text-yellow-500]
    │   │       └── [empty]   [text-muted hover:text-yellow-400]
    │   └── RateButton [w-full rounded-lg bg-primary text-primary-foreground py-2 text-sm font-medium cursor-pointer hover:bg-primary-hover transition-colors]
    └── AISummaryCard [rounded-xl border border-border bg-card p-4]
        ├── CardTitle [flex items-center gap-2 text-sm font-heading font-semibold mb-3]
        │   ├── BotIcon [w-4 h-4 text-primary]
        │   └── "Tóm tắt AI"
        ├── SummaryButton [w-full rounded-lg border border-dashed border-primary/50 text-primary text-sm py-2.5 hover:bg-primary/5 cursor-pointer transition-colors]   ← khi chưa summarize
        └── SummaryContent [text-sm text-foreground leading-relaxed space-y-2]   ← sau khi summarize
```

## Pages

### `/learning/documents` — Document List

**Filters:**
- Faculty dropdown (từ GET /faculties)
- Course dropdown (filter theo Faculty)
- File type chips: PDF / DOCX / PPTX / All
- Status filter (nếu là mod): All / Pending / Approved / Rejected

**Document Card:**
```
┌──────────────────────────────────────┐
│ [PDF] Tên tài liệu                   │
│ Môn: Phương pháp giảng dạy          │
│ Khoa: Sư phạm Toán                  │
│ [Author]  ⭐ 4.5 (23)  ↓ 156 lượt  │
│ [Approved ✓]                  [↓]   │
└──────────────────────────────────────┘
```

Status badge colors:
- Approved: `bg-success/10 text-success`
- Pending: `bg-yellow-100 text-yellow-700`
- Rejected: `bg-destructive/10 text-destructive`

### `/learning/documents/[id]` — Document Detail

```
┌──────────────────────────────────┬──────────────────┐
│  DOCUMENT VIEWER                 │  RIGHT PANEL     │
│  (PDF preview inline)            │                  │
│  hoặc File icon nếu không preview│  AI Summary      │
│                                  │  [Summarize]     │
│                                  │  <text>          │
│                                  │                  │
│                                  │  Rating: ★★★★☆   │
│                                  │  [Rate this doc] │
│                                  │                  │
│  [Download ↓]  [Report]          │  Related docs    │
└──────────────────────────────────┴──────────────────┘
```

- PDF preview: dùng `react-pdf` hoặc iframe với Google Docs viewer
- AI Summary: click button → `POST /api/v1/ai/summarize` → stream text vào panel
- Rating: star component (1-5), submit → `POST /api/v1/learning/documents/{id}/rate`

### `/learning/documents/upload` — Upload Document

**Form fields:**
- Title (required)
- Description
- Faculty (select)
- Course (select, filtered by faculty)
- File upload (drag & drop, max 50MB, accept: .pdf, .docx, .pptx, .xlsx)

**Upload flow:**
1. File upload lên Cloudinary/storage → get URL
2. Submit form với URL → `POST /api/v1/learning/documents`
3. Document vào queue Pending → thông báo "Đang chờ duyệt"

### `/learning/courses` — Course List

Cards theo faculty. Click → course detail với danh sách documents.

### `/learning/faculties` — Faculty List

Grid cards, click → filter documents theo faculty.

---

## Components to Create

```
components/features/learning/
├── DocumentCard.tsx
├── DocumentList.tsx          ← grid/list toggle + filters
├── DocumentViewer.tsx        ← PDF preview
├── UploadDocumentForm.tsx    ← drag & drop + form
├── RatingWidget.tsx          ← star rating (1-5)
├── AISummaryPanel.tsx        ← AI summary với streaming
├── CourseCard.tsx
├── FacultyCard.tsx
└── StatusBadge.tsx           ← Approved/Pending/Rejected
```

---

## Acceptance Criteria

- [ ] Document list với search + filters
- [ ] Faculty và course filters liên kết với nhau
- [ ] Upload form với drag & drop
- [ ] File size validation (max 50MB)
- [ ] PDF preview trong detail page
- [ ] Download button gọi đúng API (tracked)
- [ ] Rating widget submit được
- [ ] AI Summary button hoạt động
- [ ] Status badge hiện đúng
- [ ] Courses và Faculties page load đúng
