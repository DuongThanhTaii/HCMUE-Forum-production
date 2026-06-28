# FE-13: User Profile + Settings

| Property | Value |
|---|---|
| **ID** | FE-13 |
| **Branch** | `feature/FE-13-profile-settings` |
| **Commit** | `feat(fe/profile): implement user profile view and settings edit` |
| **Priority** | Medium |
| **Estimate** | 6h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-03 |

---

## API Endpoints

| Action | Endpoint |
|---|---|
| Get current user | GET `/api/v1/users/me` |
| Get user by ID | GET `/api/v1/users/{id}` |
| Update profile | PUT `/api/v1/users/me/profile` body: `{ firstName, lastName, bio }` |
| Get my posts | GET `/api/v1/forum/posts?authorId=me` |
| Get my documents | GET `/api/v1/learning/documents?authorId=me` |

---

## Pages

### `/profile/[id]` — Public Profile

```
┌──────────────────────────────────────────┐
│  [Avatar 80px]  Họ và Tên                │
│                 [🏫 Phòng Đào Tạo]  ← Badge│
│                 Bio text...              │
│                 Tham gia: 4/2026         │
│ ───────────────────────────────────────  │
│  Tabs: [Bài viết] [Tài liệu]            │
│  ───────────────────────────────────────│
│  [PostCard][PostCard]...                │
└──────────────────────────────────────────┘
```

**Badge display** (OfficialBadge types):
- `Department` → màu primary, icon Building
- `Faculty` → màu success, icon GraduationCap
- `Club` → màu orange, icon Users
- `Company` → màu violet, icon Briefcase
- `BoardOfDirectors` → màu accent (red), icon Crown

### `/settings` — Edit Profile

```
┌──────────────────────────────┐
│  Cài đặt tài khoản           │
│  ─────────────────────────── │
│  [Avatar]  [Đổi ảnh đại diện]│
│                              │
│  Họ:  [___________]          │
│  Tên: [___________]          │
│  Bio: [___________           │
│        ___________]          │
│                              │
│  Email: user@hcmue.edu.vn    │
│  (readonly)                  │
│                              │
│  [Lưu thay đổi]              │
│  ─────────────────────────── │
│  [Cài đặt thông báo →]       │
└──────────────────────────────┘
```

Avatar upload: Cloudinary direct upload widget.

---

## Components

```
components/features/profile/
├── UserProfile.tsx          ← public profile page
├── BadgeDisplay.tsx         ← official badge chip with icon + color
├── ProfileTabs.tsx          ← Posts / Documents tabs
└── EditProfileForm.tsx      ← settings form with RHF + Zod
```

---

## Acceptance Criteria

- [ ] Public profile hiện đúng thông tin user
- [ ] Badge hiện đúng type + màu
- [ ] Posts tab hiện bài viết của user
- [ ] Documents tab hiện tài liệu của user
- [ ] Edit profile form pre-filled với data hiện tại
- [ ] Update profile → toast success
- [ ] Avatar upload với Cloudinary
- [ ] Link "Cài đặt thông báo" → `/settings/notifications`
