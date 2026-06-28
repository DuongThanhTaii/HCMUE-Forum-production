# FE-15: Mod Zone — Learning Approvals + AI Content Queue

| Property | Value |
|---|---|
| **ID** | FE-15 |
| **Branch** | `feature/FE-15-mod-approvals` |
| **Commit** | `feat(fe/mod): implement learning document approvals and AI content moderation` |
| **Priority** | Medium |
| **Estimate** | 5h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-04, FE-08 |

---

## API Endpoints

| Action | Endpoint |
|---|---|
| Pending documents | GET `/api/v1/learning/documents?status=Pending` |
| Start review | POST `/api/v1/learning/documents/{id}/start-review` |
| Approve | POST `/api/v1/learning/documents/{id}/approve` body: `{ notes? }` |
| Reject | POST `/api/v1/learning/documents/{id}/reject` body: `{ reason }` |
| Request revision | POST `/api/v1/learning/documents/{id}/request-revision` body: `{ notes }` |
| AI flagged content | GET `/api/v1/ai/moderation/queue` (nếu có endpoint) |

---

## Pages

### `/mod/learning/approvals` — Document Approval Queue

```
┌──────────────────────────────────────────────────────┐
│  Approval Queue (5 pending)              [Refresh]   │
│  ─────────────────────────────────────────────────── │
│  [PDF] Tên tài liệu                    Submitted 1h  │
│  Môn: Phương pháp SP  Khoa: SP Toán                 │
│  By: Nguyễn Văn A                                    │
│                                                      │
│  [Preview] [Approve ✓] [Request Revision] [Reject ✗] │
│  ─────────────────────────────────────────────────── │
│  ...
└──────────────────────────────────────────────────────┘
```

**Approve:** Confirm dialog → `POST /approve`  
**Reject:** Modal với required reason field → `POST /reject`  
**Request Revision:** Modal với notes field → `POST /request-revision`  
**Preview:** Mở document viewer trong modal (PDF inline)

### `/mod/content` — AI Content Moderation Queue

Hiện các posts/comments bị AI flag với confidence score.

```
┌──────────────────────────────────────────────────────┐
│  AI Content Flags (2 pending)                        │
│  ─────────────────────────────────────────────────── │
│  [!] Post flagged — Spam (confidence: 94%)           │
│  "Mua bán tài khoản game giá rẻ..."                 │
│  By: User123  |  Forum post                          │
│  [Xem đầy đủ] [Xóa] [Override — Giữ lại]           │
└──────────────────────────────────────────────────────┘
```

---

## Components

```
components/features/mod/
├── ApprovalCard.tsx         ← single document pending approval
├── ApprovalQueue.tsx        ← list of pending documents
├── ApproveModal.tsx         ← confirm + optional notes
├── RejectModal.tsx          ← required reason
├── RevisionModal.tsx        ← notes field
├── AIFlagCard.tsx           ← AI-flagged content item
└── DocumentPreviewModal.tsx ← PDF viewer in modal
```

---

## Acceptance Criteria

- [ ] Pending documents list load đúng
- [ ] Preview document trong modal
- [ ] Approve action với confirm
- [ ] Reject action với reason required
- [ ] Request revision action với notes
- [ ] Status cập nhật sau action (optimistic + refetch)
- [ ] AI content queue hiện (nếu BE endpoint available)
- [ ] Pending counts hiện trong mod sidebar
