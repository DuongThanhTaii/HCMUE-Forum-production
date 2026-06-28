# FE-01: Design System — Tailwind + Fonts + CSS Tokens

| Property | Value |
|---|---|
| **ID** | FE-01 |
| **Branch** | `feature/FE-01-design-system` |
| **Commit** | `chore(fe): apply HCMUE design tokens and typography` |
| **Priority** | Critical — prerequisite cho tất cả tasks |
| **Estimate** | 3h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | TASK-102 (Shadcn setup — done) |

---

## Objective

Cập nhật toàn bộ design tokens trong Tailwind config và `globals.css` theo đúng [Design Spec](../superpowers/specs/2026-04-28-unihub-forum-design.md). Import fonts Poppins + Inter + JetBrains Mono.

---

## Files to Modify

### 1. `frontend/src/app/globals.css`

Cập nhật CSS custom properties theo HCMUE palette. Dùng `prefers-color-scheme` cho dark mode tự động.

```css
@import url('https://fonts.googleapis.com/css2?family=Poppins:wght@600;700&family=Inter:wght@400;500&family=JetBrains+Mono:wght@400&display=swap');

@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    /* HCMUE Brand — Light Mode */
    --color-primary: 210 74% 27%;        /* #124874 */
    --color-primary-hover: 204 70% 37%;  /* #1D6FA4 */
    --color-accent: 357 60% 51%;         /* #CF373D */
    --color-success: 142 71% 45%;        /* #16A34A */
    --background: 210 40% 98%;           /* #F8FAFC */
    --foreground: 222 47% 11%;           /* #0F172A */
    --card: 0 0% 100%;                   /* #FFFFFF */
    --card-foreground: 222 47% 11%;
    --border: 214 32% 91%;               /* #E2E8F0 */
    --input: 214 32% 91%;
    --muted: 215 16% 47%;                /* #64748B */
    --muted-foreground: 215 16% 47%;
    --primary: 210 74% 27%;
    --primary-foreground: 0 0% 100%;
    --destructive: 357 60% 51%;
    --destructive-foreground: 0 0% 100%;
    --ring: 210 74% 27%;
    --radius: 0.75rem;                   /* 12px rounded-xl for cards */

    /* Typography */
    --font-heading: 'Poppins', sans-serif;
    --font-body: 'Inter', sans-serif;
    --font-mono: 'JetBrains Mono', monospace;
  }

  .dark {
    --background: 222 47% 11%;           /* #0F172A */
    --foreground: 213 31% 91%;           /* #F1F5F9 */
    --card: 217 33% 17%;                 /* #1E293B */
    --card-foreground: 213 31% 91%;
    --border: 215 25% 27%;               /* #334155 */
    --input: 215 25% 27%;
    --muted: 215 14% 60%;                /* #94A3B8 */
    --muted-foreground: 215 14% 60%;
    --primary: 204 70% 37%;              /* #1D6FA4 — lighter in dark */
    --primary-foreground: 0 0% 100%;
    --destructive: 357 55% 56%;          /* #E05055 */
    --destructive-foreground: 0 0% 100%;
    --ring: 204 70% 37%;
  }
}

@layer base {
  * {
    @apply border-border;
  }
  body {
    @apply bg-background text-foreground;
    font-family: var(--font-body);
    font-feature-settings: "rlig" 1, "calt" 1;
  }
  h1, h2, h3, h4, h5, h6 {
    font-family: var(--font-heading);
    font-weight: 600;
  }
  code, pre, kbd {
    font-family: var(--font-mono);
  }

  /* Respect prefers-reduced-motion */
  @media (prefers-reduced-motion: reduce) {
    *, *::before, *::after {
      animation-duration: 0.01ms !important;
      transition-duration: 0.01ms !important;
    }
  }
}
```

### 2. `frontend/tailwind.config.ts`

Thêm HCMUE color aliases và font family:

```ts
import type { Config } from 'tailwindcss';

const config: Config = {
  darkMode: 'media', // system default — prefers-color-scheme
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
          hover: 'hsl(var(--color-primary-hover))',
        },
        accent: {
          DEFAULT: 'hsl(var(--color-accent))',
          foreground: 'hsl(0 0% 100%)',
        },
        success: {
          DEFAULT: 'hsl(var(--color-success))',
          foreground: 'hsl(0 0% 100%)',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        card: {
          DEFAULT: 'hsl(var(--card))',
          foreground: 'hsl(var(--card-foreground))',
        },
        // HCMUE explicit aliases
        cerulean: '#124874',
        jasper: '#CF373D',
      },
      fontFamily: {
        heading: ['var(--font-heading)'],
        body: ['var(--font-body)'],
        mono: ['var(--font-mono)'],
        sans: ['Inter', 'sans-serif'],
      },
      borderRadius: {
        lg: 'var(--radius)',           // 12px — cards
        md: 'calc(var(--radius) - 4px)', // 8px — buttons
        sm: 'calc(var(--radius) - 8px)', // 4px — tags
      },
      boxShadow: {
        card: '0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)',
        'card-hover': '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
      },
      keyframes: {
        'accordion-down': { from: { height: '0' }, to: { height: 'var(--radix-accordion-content-height)' } },
        'accordion-up': { from: { height: 'var(--radix-accordion-content-height)' }, to: { height: '0' } },
        'fade-in': { from: { opacity: '0', transform: 'translateY(4px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
      },
      animation: {
        'accordion-down': 'accordion-down 0.2s ease-out',
        'accordion-up': 'accordion-up 0.2s ease-out',
        'fade-in': 'fade-in 0.15s ease-out',
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
};
export default config;
```

### 3. `frontend/src/lib/utils/cn.ts` (verify exists)

```ts
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
```

### 4. `frontend/src/components/ui/` — Update button + card variants

Trong `button.tsx`, đảm bảo variant `default` dùng `bg-primary text-primary-foreground`.

Trong `card.tsx`, đảm bảo dùng `rounded-lg shadow-card border-border`.

---

## Layout Tree Convention (MANDATORY)

Mọi component trong dự án phải có **Tailwind Tree comment** ở đầu function body. Đây là chuẩn chung để debug layout:

```tsx
// Layout Tree:
// ComponentName [classes]
// ├── Child [classes]
// │   └── GrandChild [classes]  ← note nếu cần
// └── Child [classes]
//     ├── [state:active]  [override-classes]
//     └── [state:disabled] [override-classes]
export function ComponentName() {
```

**Rules:**
- `[classes]` = tất cả Tailwind classes của element đó theo thứ tự: layout → spacing → color → typography → interaction
- `←` = ghi chú mục đích nếu không self-evident
- State variants: `[hover:]`, `[active:]`, `[disabled:]` document trên dòng riêng
- Dynamic: `{condition ? 'class-a' : 'class-b'}` viết trong `{}`
- Chỉ cần tree cho component có ≥ 3 levels nesting

---

## Acceptance Criteria

- [ ] `pnpm dev` chạy không lỗi
- [ ] Primary color hiện thị đúng #124874 (light) trên nút `Button` default
- [ ] Background đúng #F8FAFC (light) / #0F172A (dark)
- [ ] System dark mode tự động switch khi thay đổi OS
- [ ] Font Poppins load trên heading `<h1>`
- [ ] Font Inter load trên body text
- [ ] `prefers-reduced-motion` tắt animation khi bật
- [ ] `pnpm build` không lỗi
