# 🎨 PHASE 10: FRONTEND DEVELOPMENT

> **UniHub — Next.js App với GAIA UI + PWA + Multilingual**

---

## 📋 PHASE INFO

| Property          | Value                                    |
| ----------------- | ---------------------------------------- |
| **Phase**         | 10                                       |
| **Name**          | Frontend Development (Full System)       |
| **Status**        | ⬜ NOT_STARTED                           |
| **Progress**      | 0/20 tasks                               |
| **Est. Duration** | 4-5 weeks                                |
| **Dependencies**  | Backend API (137 endpoints, Phase 3-9.7) |
| **Deployment**    | Vercel (Production + Preview)            |

---

## 🛠️ TECH STACK

### Core Framework

- **Framework**: Next.js 15.1+ (App Router)
- **Language**: TypeScript 5.7+ (strict mode)
- **Runtime**: Node.js 20+
- **Package Manager**: pnpm 9+

### UI & Styling

- **Component Library**: [GAIA UI](https://ui.heygaia.io/) (Chat/AI components)
- **Base Components**: Shadcn/ui (via GAIA UI foundation)
- **Styling**: Tailwind CSS v4
- **Animations**: Framer Motion
- **Icons**: Lucide Icons + Hugeicons (via GAIA)

### State Management

- **Server State**: TanStack Query v5 (React Query)
- **Client State**: Zustand 5
- **Form State**: React Hook Form 7 + Zod validation

### Real-time & API

- **Real-time**: SignalR Client (@microsoft/signalr)
- **HTTP Client**: Axios (with interceptors)
- **API Base**: `https://api.unihub.example` (from env)

### Additional Features

- **i18n**: next-intl (Việt + English)
- **File Upload**: Cloudinary SDK
- **PWA**: next-pwa
- **Testing**: Jest + React Testing Library
- **Linting**: ESLint + Biome
- **Code Quality**: Prettier + Husky + lint-staged

---

## 🏗️ ARCHITECTURE OVERVIEW

### Project Structure (Detailed)

```
frontend/
├── public/
│   ├── icons/              # PWA icons (192x192, 512x512)
│   ├── favicon.ico
│   ├── manifest.json
│   └── sw.js              # Service Worker
├── src/
│   ├── app/
│   │   ├── [locale]/      # i18n routing (vi/en)
│   │   │   ├── (auth)/
│   │   │   │   ├── login/
│   │   │   │   │   └── page.tsx
│   │   │   │   ├── register/
│   │   │   │   │   └── page.tsx
│   │   │   │   ├── forgot-password/
│   │   │   │   │   └── page.tsx
│   │   │   │   ├── reset-password/
│   │   │   │   │   └── page.tsx
│   │   │   │   └── layout.tsx          # Auth layout (centered form)
│   │   │   ├── (main)/
│   │   │   │   ├── layout.tsx           # Main layout (navbar + sidebar)
│   │   │   │   ├── page.tsx             # Dashboard/Home
│   │   │   │   ├── forum/
│   │   │   │   │   ├── page.tsx         # Posts list
│   │   │   │   │   ├── [id]/
│   │   │   │   │   │   ├── page.tsx     # Post detail
│   │   │   │   │   │   └── edit/page.tsx
│   │   │   │   │   ├── create/page.tsx
│   │   │   │   │   ├── categories/[id]/page.tsx
│   │   │   │   │   ├── tags/[name]/page.tsx
│   │   │   │   │   └── search/page.tsx
│   │   │   │   ├── learning/
│   │   │   │   │   ├── documents/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   ├── [id]/page.tsx
│   │   │   │   │   │   └── upload/page.tsx
│   │   │   │   │   ├── courses/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   └── [id]/page.tsx
│   │   │   │   │   ├── faculties/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   └── [id]/page.tsx
│   │   │   │   │   └── approvals/page.tsx  # Moderator only
│   │   │   │   ├── chat/
│   │   │   │   │   ├── page.tsx             # Conversations list
│   │   │   │   │   ├── [conversationId]/
│   │   │   │   │   │   └── page.tsx         # Chat window
│   │   │   │   │   ├── channels/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   └── [channelId]/page.tsx
│   │   │   │   │   └── ai-bot/
│   │   │   │   │       └── page.tsx         # UniBot chat
│   │   │   │   ├── career/
│   │   │   │   │   ├── jobs/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   ├── [id]/page.tsx
│   │   │   │   │   │   ├── create/page.tsx  # Recruiter only
│   │   │   │   │   │   └── saved/page.tsx
│   │   │   │   │   ├── companies/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   └── [id]/page.tsx
│   │   │   │   │   └── applications/
│   │   │   │   │       ├── page.tsx         # My applications
│   │   │   │   │       └── [id]/page.tsx
│   │   │   │   ├── profile/
│   │   │   │   │   ├── [userId]/
│   │   │   │   │   │   └── page.tsx
│   │   │   │   │   └── edit/page.tsx
│   │   │   │   ├── notifications/
│   │   │   │   │   └── page.tsx
│   │   │   │   └── settings/
│   │   │   │       └── page.tsx
│   │   │   └── admin/
│   │   │       ├── layout.tsx               # Admin layout
│   │   │       ├── dashboard/page.tsx
│   │   │       ├── users/page.tsx
│   │   │       ├── roles/page.tsx
│   │   │       ├── reports/page.tsx
│   │   │       └── analytics/page.tsx
│   │   ├── api/                            # API routes (proxy if needed)
│   │   │   └── auth/
│   │   │       └── [...nextauth]/route.ts
│   │   ├── layout.tsx                      # Root layout
│   │   ├── globals.css
│   │   └── providers.tsx                   # All providers
│   ├── components/
│   │   ├── ui/                             # GAIA UI + Shadcn base components
│   │   │   ├── button.tsx
│   │   │   ├── card.tsx
│   │   │   ├── chat-bubble.tsx            # GAIA
│   │   │   ├── navbar-menu.tsx            # GAIA
│   │   │   ├── raised-button.tsx          # GAIA
│   │   │   ├── tool-calls-section.tsx     # GAIA
│   │   │   ├── wave-spinner.tsx           # GAIA
│   │   │   └── ...
│   │   ├── features/                       # Feature-specific components
│   │   │   ├── auth/
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   ├── RegisterForm.tsx
│   │   │   │   ├── ForgotPasswordForm.tsx
│   │   │   │   └── ProtectedRoute.tsx
│   │   │   ├── forum/
│   │   │   │   ├── PostCard.tsx
│   │   │   │   ├── PostList.tsx
│   │   │   │   ├── PostDetail.tsx
│   │   │   │   ├── CommentSection.tsx
│   │   │   │   ├── CommentCard.tsx
│   │   │   │   ├── CreatePostForm.tsx
│   │   │   │   ├── VoteButtons.tsx
│   │   │   │   ├── TagsList.tsx
│   │   │   │   ├── CategoryFilter.tsx
│   │   │   │   └── SearchBar.tsx
│   │   │   ├── learning/
│   │   │   │   ├── DocumentCard.tsx
│   │   │   │   ├── DocumentList.tsx
│   │   │   │   ├── DocumentViewer.tsx
│   │   │   │   ├── UploadDocumentForm.tsx
│   │   │   │   ├── CourseCard.tsx
│   │   │   │   ├── CourseList.tsx
│   │   │   │   ├── FacultyCard.tsx
│   │   │   │   ├── RatingWidget.tsx
│   │   │   │   └── ApprovalQueue.tsx
│   │   │   ├── chat/
│   │   │   │   ├── ConversationList.tsx
│   │   │   │   ├── ConversationCard.tsx
│   │   │   │   ├── ChatWindow.tsx
│   │   │   │   ├── MessageBubble.tsx       # Uses GAIA chat-bubble
│   │   │   │   ├── MessageInput.tsx
│   │   │   │   ├── FileUploadPreview.tsx
│   │   │   │   ├── TypingIndicator.tsx
│   │   │   │   ├── OnlineStatus.tsx
│   │   │   │   ├── ChannelSidebar.tsx
│   │   │   │   ├── EmojiPicker.tsx
│   │   │   │   └── VoiceRecorder.tsx
│   │   │   ├── ai/
│   │   │   │   ├── UniBotChat.tsx
│   │   │   │   ├── ToolCallDisplay.tsx     # Uses GAIA tool-calls-section
│   │   │   │   ├── SummarizeButton.tsx
│   │   │   │   ├── SmartSearchBar.tsx
│   │   │   │   └── ContentModeration.tsx
│   │   │   ├── career/
│   │   │   │   ├── JobCard.tsx
│   │   │   │   ├── JobList.tsx
│   │   │   │   ├── JobDetail.tsx
│   │   │   │   ├── JobFilters.tsx
│   │   │   │   ├── CompanyCard.tsx
│   │   │   │   ├── ApplicationCard.tsx
│   │   │   │   ├── ApplicationForm.tsx
│   │   │   │   └── JobPostingForm.tsx
│   │   │   ├── notification/
│   │   │   │   ├── NotificationDropdown.tsx
│   │   │   │   ├── NotificationCard.tsx
│   │   │   │   ├── NotificationList.tsx
│   │   │   │   └── NotificationBadge.tsx
│   │   │   └── admin/
│   │   │       ├── UserManagementTable.tsx
│   │   │       ├── RoleManagementTable.tsx
│   │   │       ├── ReportsTable.tsx
│   │   │       ├── AnalyticsChart.tsx
│   │   │       └── StatsCard.tsx
│   │   └── shared/                         # Shared/common components
│   │       ├── layouts/
│   │       │   ├── Navbar.tsx             # Uses GAIA navbar-menu
│   │       │   ├── Sidebar.tsx
│   │       │   ├── Footer.tsx
│   │       │   ├── MobileMenu.tsx
│   │       │   └── Breadcrumbs.tsx
│   │       ├── FileUploader.tsx
│   │       ├── ImageUploader.tsx
│   │       ├── Avatar.tsx
│   │       ├── Badge.tsx
│   │       ├── SearchInput.tsx
│   │       ├── Pagination.tsx
│   │       ├── EmptyState.tsx
│   │       ├── ErrorBoundary.tsx
│   │       ├── LoadingSpinner.tsx         # Uses GAIA wave-spinner
│   │       ├── ConfirmDialog.tsx
│   │       ├── Toast.tsx
│   │       └── LanguageSwitcher.tsx
│   ├── hooks/
│   │   ├── auth/
│   │   │   ├── useAuth.ts                 # Auth context hook
│   │   │   ├── useLogin.ts
│   │   │   ├── useRegister.ts
│   │   │   ├── useLogout.ts
│   │   │   └── useRefreshToken.ts
│   │   ├── api/
│   │   │   ├── forum/
│   │   │   │   ├── usePosts.ts
│   │   │   │   ├── usePost.ts
│   │   │   │   ├── useCreatePost.ts
│   │   │   │   ├── useUpdatePost.ts
│   │   │   │   ├── useDeletePost.ts
│   │   │   │   ├── useVote.ts
│   │   │   │   ├── useComments.ts
│   │   │   │   ├── useCreateComment.ts
│   │   │   │   ├── useBookmark.ts
│   │   │   │   └── useTags.ts
│   │   │   ├── learning/
│   │   │   │   ├── useDocuments.ts
│   │   │   │   ├── useDocument.ts
│   │   │   │   ├── useUploadDocument.ts
│   │   │   │   ├── useDownloadDocument.ts
│   │   │   │   ├── useRateDocument.ts
│   │   │   │   ├── useCourses.ts
│   │   │   │   ├── useFaculties.ts
│   │   │   │   └── useApproveDocument.ts
│   │   │   ├── chat/
│   │   │   │   ├── useConversations.ts
│   │   │   │   ├── useMessages.ts
│   │   │   │   ├── useSendMessage.ts
│   │   │   │   ├── useChannels.ts
│   │   │   │   └── useReadReceipts.ts
│   │   │   ├── career/
│   │   │   │   ├── useJobs.ts
│   │   │   │   ├── useJob.ts
│   │   │   │   ├── useCreateJob.ts
│   │   │   │   ├── useApplications.ts
│   │   │   │   ├── useSubmitApplication.ts
│   │   │   │   └── useCompanies.ts
│   │   │   ├── notification/
│   │   │   │   ├── useNotifications.ts
│   │   │   │   ├── useMarkAsRead.ts
│   │   │   │   └── useNotificationPreferences.ts
│   │   │   └── ai/
│   │   │       ├── useAIChat.ts
│   │   │       ├── useSummarize.ts
│   │   │       ├── useSmartSearch.ts
│   │   │       └── useModerateContent.ts
│   │   ├── realtime/
│   │   │   ├── useSignalR.ts              # SignalR connection hook
│   │   │   ├── useChatHub.ts              # Chat hub
│   │   │   └── useNotificationHub.ts      # Notification hub
│   │   ├── useDebounce.ts
│   │   ├── useLocalStorage.ts
│   │   ├── useMediaQuery.ts
│   │   ├── useIntersectionObserver.ts
│   │   └── useClickOutside.ts
│   ├── lib/
│   │   ├── api/
│   │   │   ├── client.ts                  # Axios instance with interceptors
│   │   │   ├── endpoints.ts               # API endpoint constants
│   │   │   ├── auth.api.ts                # Auth endpoints
│   │   │   ├── forum.api.ts
│   │   │   ├── learning.api.ts
│   │   │   ├── chat.api.ts
│   │   │   ├── career.api.ts
│   │   │   ├── notification.api.ts
│   │   │   └── ai.api.ts
│   │   ├── signalr/
│   │   │   ├── connection.ts              # SignalR connection manager
│   │   │   ├── chatHub.ts                 # Chat hub client
│   │   │   └── notificationHub.ts         # Notification hub client
│   │   ├── cloudinary/
│   │   │   ├── upload.ts                  # Cloudinary upload helper
│   │   │   └── config.ts
│   │   ├── validations/
│   │   │   ├── auth.schema.ts             # Zod schemas for auth
│   │   │   ├── post.schema.ts
│   │   │   ├── document.schema.ts
│   │   │   ├── job.schema.ts
│   │   │   └── common.schema.ts
│   │   ├── utils/
│   │   │   ├── cn.ts                      # className merger
│   │   │   ├── date.ts                    # Date formatters
│   │   │   ├── file.ts                    # File helpers
│   │   │   ├── url.ts                     # URL builders
│   │   │   └── constants.ts
│   │   └── i18n/
│   │       ├── request.ts                 # next-intl config
│   │       └── routing.ts
│   ├── stores/
│   │   ├── auth.store.ts                  # Zustand: Auth state
│   │   ├── notification.store.ts          # Zustand: Notification state
│   │   ├── chat.store.ts                  # Zustand: Chat UI state
│   │   ├── theme.store.ts                 # Zustand: Dark mode
│   │   └── ui.store.ts                    # Zustand: Global UI state
│   ├── types/
│   │   ├── api/
│   │   │   ├── auth.types.ts
│   │   │   ├── forum.types.ts
│   │   │   ├── learning.types.ts
│   │   │   ├── chat.types.ts
│   │   │   ├── career.types.ts
│   │   │   ├── notification.types.ts
│   │   │   └── ai.types.ts
│   │   ├── common.types.ts
│   │   └── env.d.ts
│   └── middleware.ts                       # Auth + i18n middleware
├── messages/
│   ├── vi.json                            # Vietnamese translations
│   └── en.json                            # English translations
├── tests/
│   ├── components/
│   ├── hooks/
│   ├── pages/
│   └── utils/
├── .env.local.example
├── .eslintrc.json
├── .prettierrc
├── biome.json
├── next.config.ts
├── tailwind.config.ts
├── tsconfig.json
├── package.json
└── pnpm-lock.yaml
```

### Backend API Integration

**API Base URL**: `https://api.unihub.example` (configurable via `.env.local`)

**Authentication Flow**:

1. `POST /api/v1/auth/login` → Get `accessToken` + `refreshToken`
2. Store tokens in `httpOnly` cookies (or localStorage for development)
3. Axios interceptor attaches `Bearer {accessToken}` to all requests
4. On 401 response → Auto refresh using `POST /api/v1/auth/refresh`
5. If refresh fails → Redirect to `/login`

**SignalR Hubs**:

- ChatHub: `wss://api.unihub.example/hubs/chat`
- NotificationHub: `wss://api.unihub.example/hubs/notifications`
- Connection authenticated via query string: `?access_token={accessToken}`

---

## 📝 TASKS

---

## 📝 TASKS

### TASK-101: Initialize Next.js Project + Dependencies

| Property         | Value                          |
| ---------------- | ------------------------------ |
| **ID**           | TASK-101                       |
| **Status**       | ✅ COMPLETED                   |
| **Priority**     | 🔴 Critical                    |
| **Estimate**     | 4 hours                        |
| **Actual**       | 2.5 hours                      |
| **Branch**       | `feature/TASK-101-nextjs-init` |
| **Dependencies** | None                           |

**Completion Date**: 2026-02-10

**Objectives**: ✅ All completed

- ✅ Create Next.js 15 project with App Router
- ✅ Install all required dependencies (923 packages)
- ✅ Configure TypeScript, ESLint, Prettier, Biome
- ✅ Setup path aliases (@/\*)
- ✅ Configure environment variables
- ✅ Custom color palette: Cerulean Blue #124874 (primary), Jasper Red #CF373D (accent)
- ✅ Production build verified (no errors)

**Commands:**

```bash
# Create Next.js project
cd e:/ThanhTai/DHSP_HK2_25_26/Net_Web
pnpm create next-app@latest frontend --typescript --tailwind --app --src-dir --import-alias "@/*"

# Install dependencies
cd frontend
pnpm add @tanstack/react-query @tanstack/react-query-devtools
pnpm add zustand
pnpm add axios
pnpm add @microsoft/signalr
pnpm add react-hook-form @hookform/resolvers zod
pnpm add next-intl
pnpm add @cloudinary/react @cloudinary/url-gen
pnpm add lucide-react
pnpm add clsx tailwind-merge
pnpm add framer-motion
pnpm add date-fns
pnpm add sonner  # Toast notifications

# Dev dependencies
pnpm add -D @types/node @types/react @types/react-dom
pnpm add -D eslint eslint-config-next eslint-plugin-react-hooks
pnpm add -D prettier prettier-plugin-tailwindcss
pnpm add -D @biomejs/biome
pnpm add -D husky lint-staged
pnpm add -D jest @testing-library/react @testing-library/jest-dom @testing-library/user-event
pnpm add -D @types/jest jest-environment-jsdom

# PWA
pnpm add next-pwa
```

**Files to Create:**

1. `.env.local.example`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_SIGNALR_URL=http://localhost:5000/hubs
NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME=your_cloud_name
NEXT_PUBLIC_CLOUDINARY_UPLOAD_PRESET=your_upload_preset
NEXT_PUBLIC_APP_URL=http://localhost:3000
```

2. `tsconfig.json`:

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": ["DOM", "DOM.Iterable", "ES2020"],
    "jsx": "preserve",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "allowJs": true,
    "strict": true,
    "noEmit": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "incremental": true,
    "plugins": [{ "name": "next" }],
    "paths": {
      "@/*": ["./src/*"],
      "@/components/*": ["./src/components/*"],
      "@/hooks/*": ["./src/hooks/*"],
      "@/lib/*": ["./src/lib/*"],
      "@/stores/*": ["./src/stores/*"],
      "@/types/*": ["./src/types/*"]
    }
  },
  "include": ["next-env.d.ts", "**/*.ts", "**/*.tsx", ".next/types/**/*.ts"],
  "exclude": ["node_modules"]
}
```

3. `.eslintrc.json`:

```json
{
  "extends": [
    "next/core-web-vitals",
    "plugin:@tanstack/eslint-plugin-query/recommended"
  ],
  "rules": {
    "@next/next/no-html-link-for-pages": "off",
    "react/no-unescaped-entities": "off",
    "react-hooks/exhaustive-deps": "warn"
  }
}
```

4. `.prettierrc`:

```json
{
  "semi": true,
  "trailingComma": "es5",
  "singleQuote": true,
  "printWidth": 100,
  "tabWidth": 2,
  "plugins": ["prettier-plugin-tailwindcss"]
}
```

5. `biome.json`:

```json
{
  "formatter": {
    "enabled": true,
    "lineWidth": 100,
    "indentStyle": "space"
  },
  "linter": {
    "enabled": true,
    "rules": {
      "recommended": true
    }
  }
}
```

**Acceptance Criteria:**

- [ ] Next.js 15 project created with App Router
- [ ] All dependencies installed successfully
- [ ] TypeScript strict mode enabled
- [ ] Path aliases working (`@/components`, `@/hooks`, etc.)
- [ ] ESLint + Prettier configured
- [ ] `pnpm dev` runs without errors
- [ ] `.env.local` created from example

---

### TASK-102: Setup GAIA UI + Tailwind + Shadcn Base

| Property         | Value                            |
| ---------------- | -------------------------------- |
| **ID**           | TASK-102                         |
| **Status**       | ✅ COMPLETED                     |
| **Priority**     | 🔴 Critical                      |
| **Estimate**     | 3 hours                          |
| **Actual**       | 2 hours                          |
| **Branch**       | `feature/TASK-102-gaia-ui-setup` |
| **Dependencies** | TASK-101                         |

**Completion Date**: 2026-02-10

**Objectives**: ✅ All completed

- ✅ Configure Tailwind CSS v4 (already done in TASK-101)
- ✅ Install Shadcn/ui base components (Button, Card, Input, Label, Badge, Avatar, Dialog, Separator)
- ✅ Install Radix UI primitives (@radix-ui/react-\*)
- ✅ Configure component registry (components.json)
- ✅ Setup dark mode with ThemeProvider and ThemeToggle
- ✅ Update dark mode colors with Cerulean & Jasper palette
- ✅ Create component showcase page (/components)
- ✅ Production build verified

**Commands:**

```bash
# Initialize shadcn/ui
npx shadcn@latest init

# Install base components
npx shadcn@latest add button input card form dialog toast
npx shadcn@latest add avatar dropdown-menu navigation-menu
npx shadcn@latest add tabs table badge separator
npx shadcn@latest add sheet skeleton switch textarea
npx shadcn@latest add alert alert-dialog select checkbox
npx shadcn@latest add popover command scroll-area
npx shadcn@latest add label radio-group slider

# Install GAIA UI components
npx @heygaia/ui add navbar-menu
npx @heygaia/ui add chat-bubble
npx @heygaia/ui add raised-button
npx @heygaia/ui add tool-calls-section
npx @heygaia/ui add wave-spinner
```

**Files to Create/Modify:**

1. `components.json`:

```json
{
  "$schema": "https://ui.shadcn.com/schema.json",
  "style": "new-york",
  "rsc": true,
  "tsx": true,
  "tailwind": {
    "config": "tailwind.config.ts",
    "css": "src/app/globals.css",
    "baseColor": "neutral",
    "cssVariables": true
  },
  "aliases": {
    "components": "@/components",
    "utils": "@/lib/utils",
    "ui": "@/components/ui",
    "hooks": "@/hooks"
  },
  "registries": {
    "@heygaia": "https://ui.heygaia.io/r/{name}.json"
  }
}
```

2. `tailwind.config.ts`:

```ts
import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: ["class"],
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        secondary: {
          DEFAULT: "hsl(var(--secondary))",
          foreground: "hsl(var(--secondary-foreground))",
        },
        destructive: {
          DEFAULT: "hsl(var(--destructive))",
          foreground: "hsl(var(--destructive-foreground))",
        },
        muted: {
          DEFAULT: "hsl(var(--muted))",
          foreground: "hsl(var(--muted-foreground))",
        },
        accent: {
          DEFAULT: "hsl(var(--accent))",
          foreground: "hsl(var(--accent-foreground))",
        },
        popover: {
          DEFAULT: "hsl(var(--popover))",
          foreground: "hsl(var(--popover-foreground))",
        },
        card: {
          DEFAULT: "hsl(var(--card))",
          foreground: "hsl(var(--card-foreground))",
        },
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
      keyframes: {
        "accordion-down": {
          from: { height: "0" },
          to: { height: "var(--radix-accordion-content-height)" },
        },
        "accordion-up": {
          from: { height: "var(--radix-accordion-content-height)" },
          to: { height: "0" },
        },
      },
      animation: {
        "accordion-down": "accordion-down 0.2s ease-out",
        "accordion-up": "accordion-up 0.2s ease-out",
      },
    },
  },
  plugins: [require("tailwindcss-animate")],
};
export default config;
```

3. `src/app/globals.css`:

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 240 10% 3.9%;
    --card: 0 0% 100%;
    --card-foreground: 240 10% 3.9%;
    --popover: 0 0% 100%;
    --popover-foreground: 240 10% 3.9%;
    --primary: 240 5.9% 10%;
    --primary-foreground: 0 0% 98%;
    --secondary: 240 4.8% 95.9%;
    --secondary-foreground: 240 5.9% 10%;
    --muted: 240 4.8% 95.9%;
    --muted-foreground: 240 3.8% 46.1%;
    --accent: 240 4.8% 95.9%;
    --accent-foreground: 240 5.9% 10%;
    --destructive: 0 84.2% 60.2%;
    --destructive-foreground: 0 0% 98%;
    --border: 240 5.9% 90%;
    --input: 240 5.9% 90%;
    --ring: 240 5.9% 10%;
    --radius: 0.5rem;
  }

  .dark {
    --background: 240 10% 3.9%;
    --foreground: 0 0% 98%;
    --card: 240 10% 3.9%;
    --card-foreground: 0 0% 98%;
    --popover: 240 10% 3.9%;
    --popover-foreground: 0 0% 98%;
    --primary: 0 0% 98%;
    --primary-foreground: 240 5.9% 10%;
    --secondary: 240 3.7% 15.9%;
    --secondary-foreground: 0 0% 98%;
    --muted: 240 3.7% 15.9%;
    --muted-foreground: 240 5% 64.9%;
    --accent: 240 3.7% 15.9%;
    --accent-foreground: 0 0% 98%;
    --destructive: 0 62.8% 30.6%;
    --destructive-foreground: 0 0% 98%;
    --border: 240 3.7% 15.9%;
    --input: 240 3.7% 15.9%;
    --ring: 240 4.9% 83.9%;
  }
}

@layer base {
  * {
    @apply border-border;
  }
  body {
    @apply bg-background text-foreground;
    font-feature-settings:
      "rlig" 1,
      "calt" 1;
  }
}
```

4. `src/lib/utils/cn.ts`:

```ts
import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
```

**Acceptance Criteria:**

- [ ] Tailwind CSS v4 configured
- [ ] All Shadcn base components installed
- [ ] GAIA UI components installed (navbar-menu, chat-bubble, etc.)
- [ ] Dark mode CSS variables configured
- [ ] `cn()` utility working
- [ ] Test button renders with Tailwind styles

---

### TASK-103: Setup i18n (Vietnamese + English)

| Property         | Value                         |
| ---------------- | ----------------------------- |
| **ID**           | TASK-103                      |
| **Status**       | ✅ COMPLETED                  |
| **Priority**     | 🔴 Critical                   |
| **Estimate**     | 3 hours                       |
| **Actual**       | 2 hours                       |
| **Branch**       | `feature/TASK-103-i18n-setup` |
| **Dependencies** | TASK-101                      |

**Completion Date**: 2026-02-10

**Objectives**: ✅ All completed

- ✅ Configure next-intl for i18n
- ✅ Setup Vietnamese and English translations
- ✅ Create language switcher component (Globe icon with VI/EN toggle)
- ✅ Configure routing for locales (/vi/_, /en/_)
- ✅ Update next.config.ts with next-intl plugin
- ✅ Create i18n middleware for locale detection
- ✅ Restructure app directory with [locale] routing
- ✅ Expand translation files with comprehensive messages (150+ keys)
- ✅ Update component showcase with translations
- ✅ Production build verified (10 static pages generated)

**Files to Create:**

1. `src/lib/i18n/request.ts`:

```ts
import { getRequestConfig } from "next-intl/server";
import { notFound } from "next/navigation";

const locales = ["vi", "en"];

export default getRequestConfig(async ({ locale }) => {
  if (!locales.includes(locale as any)) notFound();

  return {
    messages: (await import(`../../../messages/${locale}.json`)).default,
  };
});
```

2. `src/lib/i18n/routing.ts`:

```ts
import { defineRouting } from "next-intl/routing";
import { createSharedPathnamesNavigation } from "next-intl/navigation";

export const routing = defineRouting({
  locales: ["vi", "en"],
  defaultLocale: "vi",
  localePrefix: "as-needed",
});

export const { Link, redirect, usePathname, useRouter } =
  createSharedPathnamesNavigation(routing);
```

3. `src/middleware.ts`:

```ts
import createMiddleware from "next-intl/middleware";
import { routing } from "./lib/i18n/routing";

export default createMiddleware(routing);

export const config = {
  matcher: ["/", "/(vi|en)/:path*", "/((?!api|_next|_vercel|.*\\..*).*)"],
};
```

4. `messages/vi.json`:

```json
{
  "common": {
    "loading": "Đang tải...",
    "error": "Đã xảy ra lỗi",
    "success": "Thành công",
    "save": "Lưu",
    "cancel": "Hủy",
    "delete": "Xóa",
    "edit": "Chỉnh sửa",
    "search": "Tìm kiếm",
    "filter": "Lọc",
    "noData": "Không có dữ liệu"
  },
  "auth": {
    "login": "Đăng nhập",
    "register": "Đăng ký",
    "logout": "Đăng xuất",
    "email": "Email",
    "password": "Mật khẩu",
    "forgotPassword": "Quên mật khẩu?",
    "rememberMe": "Ghi nhớ đăng nhập"
  },
  "nav": {
    "home": "Trang chủ",
    "forum": "Diễn đàn",
    "learning": "Học tập",
    "chat": "Trò chuyện",
    "career": "Nghề nghiệp",
    "profile": "Hồ sơ",
    "settings": "Cài đặt",
    "admin": "Quản trị"
  }
}
```

5. `messages/en.json`:

```json
{
  "common": {
    "loading": "Loading...",
    "error": "An error occurred",
    "success": "Success",
    "save": "Save",
    "cancel": "Cancel",
    "delete": "Delete",
    "edit": "Edit",
    "search": "Search",
    "filter": "Filter",
    "noData": "No data"
  },
  "auth": {
    "login": "Login",
    "register": "Register",
    "logout": "Logout",
    "email": "Email",
    "password": "Password",
    "forgotPassword": "Forgot password?",
    "rememberMe": "Remember me"
  },
  "nav": {
    "home": "Home",
    "forum": "Forum",
    "learning": "Learning",
    "chat": "Chat",
    "career": "Career",
    "profile": "Profile",
    "settings": "Settings",
    "admin": "Admin"
  }
}
```

6. `src/components/shared/LanguageSwitcher.tsx`:

```tsx
"use client";

import { useLocale } from "next-intl";
import { useRouter, usePathname } from "@/lib/i18n/routing";
import { Button } from "@/components/ui/button";
import { Globe } from "lucide-react";

export function LanguageSwitcher() {
  const locale = useLocale();
  const router = useRouter();
  const pathname = usePathname();

  const switchLocale = () => {
    const newLocale = locale === "vi" ? "en" : "vi";
    router.replace(pathname, { locale: newLocale });
  };

  return (
    <Button variant="ghost" size="icon" onClick={switchLocale}>
      <Globe className="h-5 w-5" />
      <span className="sr-only">Switch language</span>
    </Button>
  );
}
```

**Acceptance Criteria:**

- [ ] next-intl configured with vi/en locales
- [ ] Routes work with locale prefix (`/vi/forum`, `/en/forum`)
- [ ] Translation messages loaded correctly
- [ ] Language switcher component works
- [ ] Default locale is Vietnamese
- [ ] Middleware handles locale routing

---

### TASK-104-115: Feature Implementation (Detailed Plans)

**Chi tiết implementation được chia thành các file riêng theo module:**

| Task ID | Module                  | File                                                 | Estimate | Status |
| ------- | ----------------------- | ---------------------------------------------------- | -------- | ------ |
| **104** | **Auth & Security**     | [phase-10-auth.md](phase-10-auth.md)                 | 6h       | ✅     |
| **105** | **Layout & Navigation** | [phase-10-layout.md](phase-10-layout.md)             | 5h       | ✅     |
| **106** | **Forum Module**        | [phase-10-forum.md](phase-10-forum.md)               | 12h      | ✅     |
| **107** | **Learning Module**     | [phase-10-learning.md](phase-10-learning.md)         | 10h      | ⬜     |
| **108** | **Chat & AI Bot**       | [phase-10-chat.md](phase-10-chat.md)                 | 14h      | ⬜     |
| **109** | **Career Module**       | [phase-10-career.md](phase-10-career.md)             | 10h      | ⬜     |
| **110** | **Profile & Settings**  | [phase-10-profile.md](phase-10-profile.md)           | 6h       | ⬜     |
| **111** | **Notification Center** | [phase-10-notification.md](phase-10-notification.md) | 5h       | ⬜     |
| **112** | **Admin Dashboard**     | [phase-10-admin.md](phase-10-admin.md)               | 8h       | ⬜     |
| **113** | **PWA Features**        | [phase-10-pwa.md](phase-10-pwa.md)                   | 4h       | ⬜     |
| **114** | **Testing Setup**       | [phase-10-testing.md](phase-10-testing.md)           | 6h       | ⬜     |
| **115** | **Deployment**          | [phase-10-deployment.md](phase-10-deployment.md)     | 4h       | ⬜     |

**Total Estimate**: 90 hours (~3-4 weeks with parallel work)

---

## 🎯 DEVELOPMENT WORKFLOW

### Phase Execution Order

#### Week 1: Foundation

```
TASK-101 → TASK-102 → TASK-103 → TASK-104 → TASK-105
[Initialize] [UI Setup] [i18n] [Auth] [Layout]
```

#### Week 2-3: Core Modules (Parallel)

```
TASK-106 (Forum)      ┐
TASK-107 (Learning)   ├─→ Can work in parallel
TASK-109 (Career)     ┘

TASK-108 (Chat+AI) ──→ Requires SignalR client setup
TASK-110 (Profile) ──→ Can start after auth
TASK-111 (Notification) ──→ Depends on SignalR
```

#### Week 4: Admin & Polish

```
TASK-112 (Admin) → TASK-113 (PWA) → TASK-114 (Testing) → TASK-115 (Deploy)
```

### Git Workflow

```bash
# Create feature branch
git checkout develop
git pull origin develop
git checkout -b feature/TASK-XXX-description

# Work on feature
git add .
git commit -m "feat(TASK-XXX): description"

# Push and create PR
git push -u origin feature/TASK-XXX-description

# After PR approved → merge to develop
# After all testing → merge develop to main
```

### Code Review Checklist (Every PR)

**Functionality**

- [ ] Feature works as expected
- [ ] No console errors/warnings
- [ ] API integration working
- [ ] Error handling implemented
- [ ] Loading states present

**Code Quality**

- [ ] TypeScript strict mode compliance (no `any`)
- [ ] Component logic extracted to hooks where appropriate
- [ ] No duplicate code
- [ ] Proper naming conventions
- [ ] Comments for complex logic

**UI/UX**

- [ ] Responsive (mobile, tablet, desktop)
- [ ] Dark mode support
- [ ] Accessibility (ARIA, keyboard navigation)
- [ ] i18n strings (no hardcoded text)
- [ ] Loading spinners/skeletons
- [ ] Empty states designed
- [ ] Error states designed

**Performance**

- [ ] React.memo() for expensive components
- [ ] useMemo/useCallback where needed
- [ ] Images optimized (Next.js Image component)
- [ ] Lazy loading for routes
- [ ] No unnecessary re-renders

**Testing**

- [ ] Unit tests for utility functions
- [ ] Component tests for key interactions
- [ ] E2E tests for critical user flows

---

## ✅ COMPLETION CHECKLIST

### Phase 10.1: Foundation (Week 1)

- [ ] **TASK-101**: Next.js project + dependencies installed
- [ ] **TASK-102**: GAIA UI + Tailwind configured
- [ ] **TASK-103**: i18n (vi/en) working with language switcher
- [ ] **TASK-104**: Auth pages complete (login, register, forgot/reset password)
- [ ] **TASK-105**: Main layout with navbar, sidebar, footer responsive

### Phase 10.2: Core Modules (Week 2-3)

- [ ] **TASK-106**: Forum complete (posts list/detail, create/edit, comments, voting, tags, search)
- [ ] **TASK-107**: Learning complete (documents list/detail/upload, courses, faculties, approvals)
- [ ] **TASK-108**: Chat + AI Bot complete (conversations, channels, real-time messaging, UniBot)
- [ ] **TASK-109**: Career complete (jobs list/detail/post, companies, applications, saved jobs)
- [ ] **TASK-110**: Profile complete (view profile, edit profile, change password, settings)
- [ ] **TASK-111**: Notification center complete (dropdown, page, real-time SignalR updates)

### Phase 10.3: Admin & Polish (Week 4)

- [ ] **TASK-112**: Admin dashboard complete (users, roles, reports, analytics)
- [ ] **TASK-113**: PWA configured (manifest, service worker, offline support, installable)
- [ ] **TASK-114**: Testing setup complete (Jest, RTL, test coverage > 70%)
- [ ] **TASK-115**: Deployed to Vercel (production domain + preview deployments)

### Quality Gates (Final)

- [ ] All pages responsive (mobile 375px → desktop 1920px)
- [ ] Dark mode works across entire app
- [ ] i18n complete for all user-facing strings (vi + en)
- [ ] All forms validated with Zod schemas
- [ ] Error boundaries catch all errors gracefully
- [ ] Loading states for all async operations (queries, mutations)
- [ ] Lighthouse audit: Performance > 90, Accessibility > 95, Best Practices > 90, SEO > 90
- [ ] Core Web Vitals green (LCP < 2.5s, FID < 100ms, CLS < 0.1)
- [ ] PWA audit passed (installable, works offline for key pages)
- [ ] No TypeScript errors (`pnpm tsc --noEmit`)
- [ ] No ESLint errors (`pnpm lint`)
- [ ] All unit tests pass (`pnpm test`)

---

## 📚 REFERENCE DOCUMENTATION

### Official Docs

- [Next.js 15 Documentation](https://nextjs.org/docs)
- [React 19 Documentation](https://react.dev/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)

### UI & Styling

- [GAIA UI Component Gallery](https://ui.heygaia.io/)
- [Shadcn/ui Components](https://ui.shadcn.com/)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [Radix UI Primitives](https://www.radix-ui.com/primitives)
- [Framer Motion](https://www.framer.com/motion/)
- [Lucide Icons](https://lucide.dev/)

### State & Data

- [TanStack Query v5](https://tanstack.com/query/latest)
- [Zustand Documentation](https://github.com/pmndrs/zustand)
- [React Hook Form](https://react-hook-form.com/)
- [Zod Validation](https://zod.dev/)

### Real-time & API

- [SignalR JavaScript Client](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [Axios Documentation](https://axios-http.com/docs/intro)

### Internationalization

- [next-intl Documentation](https://next-intl-docs.vercel.app/)

### File Upload

- [Cloudinary React SDK](https://cloudinary.com/documentation/react_integration)

### PWA

- [next-pwa](https://github.com/shadowwalker/next-pwa)
- [Web App Manifest](https://developer.mozilla.org/en-US/docs/Web/Manifest)
- [Service Worker API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)

### Testing

- [Jest Documentation](https://jestjs.io/docs/getting-started)
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/)

### Deployment

- [Vercel Documentation](https://vercel.com/docs)
- [Next.js Deployment](https://nextjs.org/docs/app/building-your-application/deploying)

---

## 🚀 QUICK START GUIDE

### Initial Setup

```bash
# 1. Clone repository (if not already)
cd e:/ThanhTai/DHSP_HK2_25_26/Net_Web

# 2. Initialize Next.js project (TASK-101)
pnpm create next-app@latest frontend --typescript --tailwind --app --src-dir --import-alias "@/*"

# 3. Navigate to frontend
cd frontend

# 4. Install all dependencies
pnpm install

# 5. Setup environment variables
cp .env.local.example .env.local

# 6. Edit .env.local with actual values
code .env.local
```

### Environment Variables Template

```env
# API Configuration
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_SIGNALR_URL=http://localhost:5000/hubs

# Cloudinary (File uploads)
NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME=your_cloud_name
NEXT_PUBLIC_CLOUDINARY_UPLOAD_PRESET=your_upload_preset
NEXT_PUBLIC_CLOUDINARY_API_KEY=your_api_key

# App Configuration
NEXT_PUBLIC_APP_URL=http://localhost:3000
NEXT_PUBLIC_APP_NAME=UniHub
NEXT_PUBLIC_DEFAULT_LOCALE=vi

# Feature Flags (optional)
NEXT_PUBLIC_ENABLE_PWA=true
NEXT_PUBLIC_ENABLE_ANALYTICS=false
```

### Development Commands

```bash
# Run development server
pnpm dev

# Build for production
pnpm build

# Start production server
pnpm start

# Type check
pnpm tsc --noEmit

# Lint code
pnpm lint

# Format code
pnpm format

# Run tests
pnpm test

# Run tests with coverage
pnpm test:coverage

# Run E2E tests
pnpm test:e2e
```

### Verify Setup

```bash
# 1. Run dev server
pnpm dev

# 2. Open browser
http://localhost:3000

# 3. Check API connection
http://localhost:3000/api/health (should proxy to backend)

# 4. Test language switcher
http://localhost:3000/vi → http://localhost:3000/en

# 5. Check dark mode toggle
Click moon/sun icon in navbar
```

---

## 📞 SUPPORT & TROUBLESHOOTING

### Common Issues

**Issue**: `Module not found: Can't resolve '@/components/...'`

- **Fix**: Check `tsconfig.json` paths are configured correctly
- Restart TypeScript server in VS Code (Cmd+Shift+P → "TypeScript: Restart TS Server")

**Issue**: CORS errors when calling backend API

- **Fix**: Ensure backend CORS policy includes `http://localhost:3000`
- Check backend is running on `http://localhost:5000`

**Issue**: SignalR connection fails

- **Fix**: Check SignalR URL in `.env.local`
- Ensure backend SignalR hubs are running
- Check network tab for WebSocket connection

**Issue**: Translations not loading

- **Fix**: Check `messages/vi.json` and `messages/en.json` exist
- Verify `src/middleware.ts` is configured
- Clear Next.js cache: `rm -rf .next`

**Issue**: Tailwind styles not applying

- **Fix**: Check `tailwind.config.ts` content paths include all files
- Restart dev server
- Clear browser cache

---

_Last Updated: 2026-02-10_
_Next Update: After TASK-101 completion_
