# FE-02: Auth Pages — Login / Register / Forgot / Reset Password

| Property | Value |
|---|---|
| **ID** | FE-02 |
| **Branch** | `feature/FE-02-auth-pages` |
| **Commit** | `feat(fe/auth): implement login, register, forgot and reset password pages` |
| **Priority** | Critical |
| **Estimate** | 6h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-01 |

---

## Objective

Build 4 auth pages với layout căn giữa, gradient background HCMUE, form validation với Zod + React Hook Form, kết nối API BE (`/api/v1/auth/*`).

---

## API Endpoints

| Action | Method | Endpoint |
|---|---|---|
| Login | POST | `/api/v1/auth/login` |
| Register | POST | `/api/v1/auth/register` |
| Forgot password | POST | `/api/v1/auth/forgot-password` |
| Reset password | POST | `/api/v1/auth/reset-password` |
| Refresh token | POST | `/api/v1/auth/refresh` |

---

## Tailwind Layout Tree

```
LoginPage [contents]
└── (inherits AuthRoot từ layout.tsx — xem FE-03)
    └── AuthCardBody [px-8 pb-8]
        └── LoginForm [space-y-4]
            ├── FormField [space-y-1.5]
            │   ├── Label [text-sm font-medium text-foreground]
            │   └── Input [w-full rounded-lg border border-border bg-background px-3 py-2 text-sm placeholder:text-muted focus:ring-2 focus:ring-primary/30 focus:border-primary outline-none transition-all]
            │       └── [error] [border-destructive focus:ring-destructive/30]
            ├── FormField (password) [space-y-1.5 relative]
            │   ├── LabelRow [flex items-center justify-between]
            │   │   ├── Label [text-sm font-medium]
            │   │   └── ForgotLink [text-xs text-primary hover:underline cursor-pointer]
            │   └── PasswordInput [relative]
            │       ├── Input [pr-10 ...same as above...]
            │       └── ToggleVisibility [absolute right-3 top-1/2 -translate-y-1/2 text-muted hover:text-foreground cursor-pointer]
            ├── RememberRow [flex items-center gap-2]
            │   ├── Checkbox [w-4 h-4 rounded border-border]
            │   └── CheckboxLabel [text-sm text-foreground cursor-pointer]
            ├── ErrorMessage? [flex items-center gap-2 rounded-lg bg-destructive/10 border border-destructive/20 px-3 py-2 text-sm text-destructive]
            ├── SubmitButton [w-full rounded-lg bg-primary text-primary-foreground py-2.5 text-sm font-medium hover:bg-primary-hover transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed]
            │   └── [loading] [flex items-center justify-center gap-2]
            │       └── Spinner [w-4 h-4 animate-spin]
            └── RegisterLink [text-center text-sm text-muted mt-2]
                └── Link [text-primary hover:underline cursor-pointer font-medium]
```

## Layout: `(auth)/layout.tsx`

```tsx
// Full-page gradient: từ #124874 → #1D6FA4 (diagonal)
// Center card: bg-card rounded-xl shadow-lg p-8 max-w-md w-full
// Trên card: Logo UniHub · HCMUE + tagline
```

### Skeleton layout:
```
┌─────────────────────────────────┐
│  gradient bg (#124874 → #1D6FA4)│
│                                  │
│      ┌──────────────────────┐   │
│      │ [U] UniHub · HCMUE   │   │
│      │ "Cộng đồng ĐHSP"     │   │
│      │────────────────────  │   │
│      │  <Form content>      │   │
│      └──────────────────────┘   │
│                                  │
│  © 2026 UniHub · HCMUE          │
└─────────────────────────────────┘
```

---

## Pages

### `/[locale]/(auth)/login/page.tsx`

**Form fields:**
- Email (required, email format)
- Password (required, min 6)
- Checkbox "Ghi nhớ đăng nhập"

**Actions:**
- Submit → `POST /api/v1/auth/login` → lưu token vào cookie httpOnly (hoặc Zustand + sessionStorage trong dev)
- Link "Quên mật khẩu?" → `/forgot-password`
- Link "Đăng ký tài khoản" → `/register`

**Zod schema:**
```ts
const loginSchema = z.object({
  email: z.string().email('Email không hợp lệ'),
  password: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
});
```

### `/[locale]/(auth)/register/page.tsx`

**Form fields:**
- First name (required)
- Last name (required)
- Email (required, email format)
- Password (required, min 8, regex: chữ hoa + số)
- Confirm password (match)

**Zod schema:**
```ts
const registerSchema = z.object({
  firstName: z.string().min(1, 'Họ là bắt buộc'),
  lastName: z.string().min(1, 'Tên là bắt buộc'),
  email: z.string().email('Email không hợp lệ'),
  password: z.string()
    .min(8, 'Mật khẩu tối thiểu 8 ký tự')
    .regex(/^(?=.*[A-Z])(?=.*\d)/, 'Cần ít nhất 1 chữ hoa và 1 số'),
  confirmPassword: z.string(),
}).refine(d => d.password === d.confirmPassword, {
  message: 'Mật khẩu không khớp',
  path: ['confirmPassword'],
});
```

### `/[locale]/(auth)/forgot-password/page.tsx`

**Form fields:** Email only  
**On submit:** `POST /api/v1/auth/forgot-password` → hiện success message "Kiểm tra email của bạn"

### `/[locale]/(auth)/reset-password/page.tsx`

**Query params:** `?token=xxx`  
**Form fields:** New password + confirm password  
**On submit:** `POST /api/v1/auth/reset-password` → redirect `/login`

---

## Files to Create/Update

```
frontend/src/
├── app/[locale]/(auth)/
│   ├── layout.tsx              ← Auth layout (gradient bg)
│   ├── login/page.tsx
│   ├── register/page.tsx
│   ├── forgot-password/page.tsx
│   └── reset-password/page.tsx
├── components/features/auth/
│   ├── LoginForm.tsx
│   ├── RegisterForm.tsx
│   ├── ForgotPasswordForm.tsx
│   └── ResetPasswordForm.tsx
├── lib/
│   ├── api/auth.api.ts         ← axios calls
│   └── validations/auth.schema.ts ← Zod schemas
├── stores/auth.store.ts        ← Zustand: user, tokens
└── hooks/auth/
    ├── useAuth.ts
    ├── useLogin.ts
    ├── useRegister.ts
    └── useForgotPassword.ts
```

## Key Implementation Notes

- Tokens lưu trong cookie `httpOnly` (production). Dev: `localStorage` acceptable.
- Axios interceptor tự động attach `Authorization: Bearer <token>` — setup trong `lib/api/client.ts`.
- On 401 response → auto call refresh token → retry once → nếu fail redirect `/login`.
- Redirect sau login: kiểm tra `callbackUrl` query param, fallback `/`.
- `useAuth()` hook expose: `{ user, isAuthenticated, isLoading }`.

---

## Acceptance Criteria

- [ ] Login form submit → call API → redirect home khi thành công
- [ ] Register form submit → call API → redirect login
- [ ] Forgot password → hiện success message
- [ ] Validation errors hiện inline dưới field
- [ ] Loading spinner trong button khi submitting
- [ ] Error toast (Sonner) khi API trả lỗi (400/401/500)
- [ ] Token lưu và được gửi trong header của request tiếp theo
- [ ] Đã logout → truy cập route protected → redirect `/login`
- [ ] i18n: tất cả text qua `t()` từ `messages/vi.json`
