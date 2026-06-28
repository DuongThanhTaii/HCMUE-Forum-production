# FE-03: (main) Layout — Left Sidebar + AppShell

| Property | Value |
|---|---|
| **ID** | FE-03 |
| **Branch** | `feature/FE-03-main-layout` |
| **Commit** | `feat(fe/layout): implement main app shell with left sidebar` |
| **Priority** | Critical |
| **Estimate** | 8h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-01, FE-02 |

---

## Objective

Xây dựng layout chính cho Zone 1 (user-facing). Dùng Shadcn `SidebarProvider` + `Sidebar`. Sidebar 240px cố định trên desktop.

---

## Layout Skeleton

```
┌────────────────────────────────────────────────────────┐
│  SIDEBAR (240px)          │  MAIN CONTENT (flex-1)    │
│ ┌──────────────────────┐  │ ┌──────────────────────┐  │
│ │ [U] UniHub · HCMUE   │  │ │  {children}          │  │
│ │──────────────────────│  │ │                      │  │
│ │ [Search Cmd+K]       │  │ └──────────────────────┘  │
│ │──────────────────────│  │                           │
│ │ Home                 │  │                           │
│ │ Forum                │  │                           │
│ │ Learning             │  │                           │
│ │ Career               │  │                           │
│ │ Chat          [3]    │  │                           │
│ │──────────────────────│  │                           │
│ │ AI Assistant         │  │                           │
│ │──────────────────────│  │                           │
│ │ [Avatar] Họ Tên      │  │                           │
│ │ [Bell][Settings][...] │  │                           │
│ └──────────────────────┘  │                           │
└────────────────────────────────────────────────────────┘
```

## Tailwind Layout Tree (Full)

Đây là tree đầy đủ cho `(main)` layout — phải match 1:1 với code:

```
(main)/layout.tsx
└── SidebarProvider [flex h-screen overflow-hidden bg-background]
    ├── AppSidebar                                              ← Shadcn Sidebar, w-60
    │   ├── SidebarHeader [h-14 flex items-center px-4 border-b border-border shrink-0]
    │   │   └── SidebarLogo [flex items-center gap-2.5 cursor-pointer select-none]
    │   │       ├── LogoIconBox [w-7 h-7 rounded-lg bg-primary flex items-center justify-center shrink-0]
    │   │       │   └── LogoIcon [w-4 h-4 text-primary-foreground]     ← SVG
    │   │       └── LogoWordmark [flex flex-col leading-none]
    │   │           ├── Title [text-sm font-heading font-bold text-foreground]  ← "UniHub"
    │   │           └── Sub [text-[10px] text-muted uppercase tracking-wide]   ← "HCMUE"
    │   ├── SidebarSearch [px-3 py-2 shrink-0]
    │   │   └── SearchButton [flex items-center gap-2 w-full rounded-lg border border-border bg-muted/40 px-3 py-1.5 text-sm text-muted cursor-pointer hover:bg-muted/70 transition-colors duration-150]
    │   │       ├── SearchIcon [w-3.5 h-3.5]
    │   │       ├── SearchLabel [flex-1 text-left]                     ← "Tìm kiếm..."
    │   │       └── KbdBadge [text-[10px] bg-background border border-border rounded px-1 py-0.5]  ← "⌘K"
    │   ├── SidebarContent [flex-1 overflow-y-auto px-2 py-2 space-y-0.5]
    │   │   └── NavItem (×5) [group flex items-center gap-3 rounded-lg px-3 py-2 text-sm cursor-pointer transition-colors duration-150 select-none]
    │   │       ├── [active]   [bg-primary/10 text-primary font-medium]
    │   │       ├── [inactive] [text-foreground hover:bg-muted/60]
    │   │       ├── NavIcon [w-4 h-4 shrink-0]
    │   │       ├── NavLabel [flex-1 truncate]
    │   │       └── NavBadge? [ml-auto text-[10px] font-semibold bg-accent text-white rounded-full min-w-[18px] h-[18px] flex items-center justify-center px-1]
    │   ├── SidebarDivider [mx-3 border-t border-border my-1 shrink-0]
    │   ├── SidebarSecondary [px-2 py-1 shrink-0]
    │   │   └── NavItem (AI) [flex items-center gap-3 rounded-lg px-3 py-2 text-sm cursor-pointer transition-colors duration-150]
    │   │       ├── [active]   [bg-primary/10 text-primary font-medium]
    │   │       └── [inactive] [text-muted hover:bg-muted/60 hover:text-foreground]
    │   └── SidebarFooter [px-3 py-3 border-t border-border shrink-0]
    │       └── UserRow [flex items-center gap-2.5]
    │           ├── Avatar [w-8 h-8 rounded-full ring-2 ring-border overflow-hidden shrink-0]
    │           ├── UserInfo [flex-1 min-w-0]
    │           │   ├── UserName [text-sm font-medium text-foreground truncate leading-none mb-0.5]
    │           │   └── UserBadge? [text-[10px] text-muted truncate]   ← badge type nếu có
    │           └── FooterActions [flex items-center gap-0.5 shrink-0]
    │               ├── BellButton [relative p-1.5 rounded-md text-muted hover:text-foreground hover:bg-muted/60 cursor-pointer transition-colors]
    │               │   ├── BellIcon [w-4 h-4]
    │               │   └── UnreadDot? [absolute -top-0.5 -right-0.5 w-4 h-4 rounded-full bg-accent text-[9px] text-white flex items-center justify-center font-bold]
    │               └── SettingsButton [p-1.5 rounded-md text-muted hover:text-foreground hover:bg-muted/60 cursor-pointer transition-colors]
    │                   └── SettingsIcon [w-4 h-4]
    └── SidebarInset [flex flex-col flex-1 min-w-0 overflow-hidden]
        └── main [flex-1 overflow-y-auto]
            └── PageWrapper [p-6 max-w-screen-xl mx-auto]              ← {children}
```

### Auth layout tree

```
(auth)/layout.tsx
└── AuthRoot [min-h-screen flex flex-col items-center justify-center bg-gradient-to-br from-primary to-primary-hover p-4]
    ├── AuthCard [w-full max-w-md bg-card rounded-2xl shadow-2xl overflow-hidden]
    │   ├── AuthCardHeader [px-8 pt-8 pb-6 text-center]
    │   │   ├── LogoGroup [flex items-center justify-center gap-2 mb-4]
    │   │   │   ├── LogoIcon [w-9 h-9 rounded-xl bg-primary flex items-center justify-center]
    │   │   │   └── LogoText [text-lg font-heading font-bold text-foreground]
    │   │   ├── PageTitle [text-xl font-heading font-bold text-foreground]
    │   │   └── PageSubtitle [text-sm text-muted mt-1]
    │   └── AuthCardBody [px-8 pb-8]
    │       └── {children}                                              ← form
    └── AuthFooter [mt-6 text-center text-xs text-white/70]
        └── "© 2026 UniHub · HCMUE"
```

### Admin layout tree

```
(admin)/layout.tsx
└── AdminRoot [flex flex-col h-screen overflow-hidden bg-background]
    ├── AdminTopBar [h-14 flex items-center px-6 border-b border-border bg-card shrink-0]
    │   ├── AdminLogo [flex items-center gap-2 mr-8]
    │   │   └── "Admin Panel" [text-sm font-heading font-semibold]
    │   ├── TopBarSpacer [flex-1]
    │   └── TopBarRight [flex items-center gap-3]
    │       ├── UserChip [flex items-center gap-2 text-sm]
    │       └── LogoutButton [text-sm text-muted hover:text-foreground cursor-pointer]
    └── AdminBody [flex flex-1 overflow-hidden]
        ├── AdminNav [w-44 shrink-0 border-r border-border bg-card overflow-y-auto py-4]
        │   └── NavGroup [mb-4]
        │       ├── NavGroupLabel [px-3 text-[10px] uppercase tracking-wider text-muted mb-1]
        │       └── NavItem (×n) [flex items-center gap-2.5 px-3 py-2 text-sm rounded-lg mx-2 cursor-pointer transition-colors]
        │           ├── [active]   [bg-primary/10 text-primary font-medium]
        │           └── [inactive] [text-foreground hover:bg-muted/60]
        └── AdminContent [flex-1 overflow-y-auto p-6]
            └── {children}
```

---

## Files to Create

```
frontend/src/
├── app/[locale]/(main)/
│   └── layout.tsx                    ← Zone 1 layout
├── components/shared/layouts/
│   ├── AppSidebar.tsx                ← Shadcn Sidebar wrapper
│   ├── SidebarNav.tsx                ← Navigation items
│   ├── SidebarUserFooter.tsx         ← Avatar + action buttons
│   ├── SidebarLogo.tsx               ← UniHub · HCMUE logo
│   └── GlobalSearch.tsx             ← Cmd+K command palette
```

---

## Implementation Details

### `app/[locale]/(main)/layout.tsx`

```tsx
import { SidebarProvider, SidebarInset } from '@/components/ui/sidebar';
import { AppSidebar } from '@/components/shared/layouts/AppSidebar';

export default function MainLayout({ children }: { children: React.ReactNode }) {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <main className="flex-1 overflow-y-auto p-6">
          {children}
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
}
```

### `AppSidebar.tsx` — Navigation Items

```ts
const navItems = [
  { href: '/', icon: Home, label: 'nav.home' },
  { href: '/forum', icon: MessageSquare, label: 'nav.forum' },
  { href: '/learning/documents', icon: BookOpen, label: 'nav.learning' },
  { href: '/career/jobs', icon: Briefcase, label: 'nav.career' },
  { href: '/chat', icon: MessageCircle, label: 'nav.chat', badge: unreadCount },
];

const secondaryItems = [
  { href: '/chat/ai', icon: Bot, label: 'nav.aiAssistant' },
];
```

Active state: `bg-primary/10 text-primary font-medium` pada item aktif.  
Hover state: `hover:bg-muted cursor-pointer transition-colors duration-150`.

### `SidebarUserFooter.tsx`

Hiện ở bottom sidebar:
- Avatar (từ `useAuth().user`)
- Tên user (truncate nếu dài)
- Badge icon nếu có OfficialBadge
- Icon Bell (notification count badge đỏ)
- Icon Settings → `/settings`
- Dropdown menu: Profile / Settings / Logout

### `GlobalSearch.tsx` — Cmd+K

- Dùng Shadcn `Command` component
- Trigger: `Ctrl+K` / `Cmd+K`
- Khi gõ: debounce 300ms → gọi Smart Search AI (`POST /api/v1/ai/search`)
- Hiện results: Posts, Documents, Jobs grouped
- Keyboard navigation (↑↓ Enter)

### Auth Guard trong layout

```tsx
// Trong layout.tsx
import { redirect } from 'next/navigation';
import { getServerSession } from '@/lib/auth/session';

// Nếu không có session → redirect login
const session = await getServerSession();
if (!session) redirect(`/${locale}/login`);
```

### Middleware Update

`src/middleware.ts` — thêm auth check cho (main), (mod), (admin) routes:

```ts
// Protected routes: /vi/(main)/*, /vi/(mod)/*, /vi/(admin)/*
// Public routes: /vi/(auth)/*
```

---

## Acceptance Criteria

- [ ] Sidebar render đúng 240px trên desktop
- [ ] Active nav item highlight với primary color
- [ ] Unread chat count badge hiện đúng
- [ ] User avatar + tên hiện ở bottom sidebar
- [ ] Bell icon có unread notification badge
- [ ] Cmd+K mở search palette
- [ ] Search trả về kết quả từ API
- [ ] Unauthenticated user bị redirect về login
- [ ] i18n: tất cả label sidebar qua `t()`
- [ ] `pnpm build` không lỗi
