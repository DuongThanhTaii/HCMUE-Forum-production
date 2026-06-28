# TASK-104: Auth & Security Module

> **Authentication flow, protected routes, JWT token management**

---

## 📋 TASK INFO

| Property         | Value                         |
| ---------------- | ----------------------------- |
| **Task ID**      | TASK-104                      |
| **Module**       | Authentication & Security     |
| **Status**       | ✅ COMPLETED                  |
| **Priority**     | 🔴 Critical                   |
| **Estimate**     | 6 hours                       |
| **Actual**       | 4 hours                       |
| **Branch**       | `feature/TASK-104-auth-pages` |
| **Dependencies** | TASK-101, TASK-102, TASK-103  |

**Completion Date**: 2026-02-10

**Objectives**: ✅ All completed

- ✅ Implement login/register pages with form validation
- ✅ Setup JWT token management (access + refresh)
- ✅ Create Zustand auth store with persistence
- ✅ Implement protected route wrapper
- ✅ Setup Axios client with JWT interceptors
- ✅ Integrate with backend AUTH endpoints structure
- ✅ Add React Query provider for data fetching
- ✅ Add Sonner toast notifications

---

## 📡 BACKEND API ENDPOINTS

### Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}

Response 200:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "dGVzdC...",
  "expiresIn": 900,
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "fullName": "Nguyễn Văn A",
    "roles": ["Student"]
  }
}
```

### Register

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "Password123!",
  "fullName": "Nguyễn Văn B",
  "studentId": "20210001"
}

Response 200:
{
  "accessToken": "...",
  "refreshToken": "...",
  "expiresIn": 900,
  "user": { ... }
}
```

### Refresh Token

```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "dGVzdC..."
}

Response 200:
{
  "accessToken": "new_token...",
  "refreshToken": "new_refresh...",
  "expiresIn": 900
}
```

### Logout

```http
POST /api/v1/auth/logout
Authorization: Bearer {accessToken}

Response 204: No Content
```

### Forgot Password

```http
POST /api/v1/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}

Response 200:
{
  "message": "Password reset email sent"
}
```

### Reset Password

```http
POST /api/v1/auth/reset-password
Content-Type: application/json

{
  "token": "reset_token_from_email",
  "newPassword": "NewPassword123!"
}

Response 200:
{
  "message": "Password reset successfully"
}
```

---

## 📁 FILES TO CREATE

### 1. Auth Store (Zustand)

**File**: `src/stores/auth.store.ts`

```typescript
import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface User {
  id: string;
  email: string;
  fullName: string;
  avatar?: string;
  roles: string[];
  studentId?: string;
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;

  // Actions
  setAuth: (data: {
    user: User;
    accessToken: string;
    refreshToken: string;
  }) => void;
  clearAuth: () => void;
  updateUser: (user: Partial<User>) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: ({ user, accessToken, refreshToken }) =>
        set({
          user,
          accessToken,
          refreshToken,
          isAuthenticated: true,
        }),

      clearAuth: () =>
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        }),

      updateUser: (userData) =>
        set((state) => ({
          user: state.user ? { ...state.user, ...userData } : null,
        })),
    }),
    {
      name: "auth-storage",
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
);
```

### 2. Auth API Client

**File**: `src/lib/api/auth.api.ts`

```typescript
import { apiClient } from "./client";

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  studentId?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: {
    id: string;
    email: string;
    fullName: string;
    avatar?: string;
    roles: string[];
    studentId?: string;
  };
}

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResponse>("/api/v1/auth/login", data),

  register: (data: RegisterRequest) =>
    apiClient.post<AuthResponse>("/api/v1/auth/register", data),

  refreshToken: (refreshToken: string) =>
    apiClient.post<AuthResponse>("/api/v1/auth/refresh", { refreshToken }),

  logout: () => apiClient.post("/api/v1/auth/logout"),

  forgotPassword: (email: string) =>
    apiClient.post("/api/v1/auth/forgot-password", { email }),

  resetPassword: (token: string, newPassword: string) =>
    apiClient.post("/api/v1/auth/reset-password", { token, newPassword }),
};
```

### 3. Axios Client with Interceptors

**File**: `src/lib/api/client.ts`

```typescript
import axios from "axios";
import { useAuthStore } from "@/stores/auth.store";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

export const apiClient = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor: Add access token
apiClient.interceptors.request.use(
  (config) => {
    const { accessToken } = useAuthStore.getState();
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

// Response interceptor: Handle 401 with refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If 401 and not already retried
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const { refreshToken, setAuth, clearAuth } = useAuthStore.getState();

        if (!refreshToken) {
          clearAuth();
          window.location.href = "/vi/login";
          return Promise.reject(error);
        }

        // Attempt to refresh token
        const response = await axios.post(`${API_URL}/api/v1/auth/refresh`, {
          refreshToken,
        });

        const {
          accessToken,
          refreshToken: newRefreshToken,
          user,
        } = response.data;

        // Update store with new tokens
        setAuth({ user, accessToken, refreshToken: newRefreshToken });

        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed → logout
        useAuthStore.getState().clearAuth();
        window.location.href = "/vi/login";
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  },
);
```

### 4. Auth Hooks

**File**: `src/hooks/auth/useAuth.ts`

```typescript
import { useAuthStore } from "@/stores/auth.store";

export function useAuth() {
  const { user, isAuthenticated, clearAuth } = useAuthStore();

  const hasRole = (role: string) => {
    return user?.roles.includes(role) || false;
  };

  const hasAnyRole = (roles: string[]) => {
    return roles.some((role) => user?.roles.includes(role));
  };

  const isAdmin = () => hasRole("Admin");
  const isModerator = () => hasAnyRole(["Admin", "Moderator"]);

  return {
    user,
    isAuthenticated,
    logout: clearAuth,
    hasRole,
    hasAnyRole,
    isAdmin,
    isModerator,
  };
}
```

**File**: `src/hooks/auth/useLogin.ts`

```typescript
import { useMutation } from "@tanstack/react-query";
import { useRouter } from "@/lib/i18n/routing";
import { authApi, type LoginRequest } from "@/lib/api/auth.api";
import { useAuthStore } from "@/stores/auth.store";
import { toast } from "sonner";
import { useTranslations } from "next-intl";

export function useLogin() {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);
  const t = useTranslations("auth");

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (response) => {
      setAuth({
        user: response.data.user,
        accessToken: response.data.accessToken,
        refreshToken: response.data.refreshToken,
      });
      toast.success(t("loginSuccess"));
      router.push("/");
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || t("loginError");
      toast.error(message);
    },
  });
}
```

**File**: `src/hooks/auth/useRegister.ts`

```typescript
import { useMutation } from "@tanstack/react-query";
import { useRouter } from "@/lib/i18n/routing";
import { authApi, type RegisterRequest } from "@/lib/api/auth.api";
import { useAuthStore } from "@/stores/auth.store";
import { toast } from "sonner";
import { useTranslations } from "next-intl";

export function useRegister() {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);
  const t = useTranslations("auth");

  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
    onSuccess: (response) => {
      setAuth({
        user: response.data.user,
        accessToken: response.data.accessToken,
        refreshToken: response.data.refreshToken,
      });
      toast.success(t("registerSuccess"));
      router.push("/");
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || t("registerError");
      toast.error(message);
    },
  });
}
```

### 5. Zod Validation Schemas

**File**: `src/lib/validations/auth.schema.ts`

```typescript
import { z } from "zod";

export const loginSchema = z.object({
  email: z.string().email("Email không hợp lệ"),
  password: z.string().min(8, "Mật khẩu phải có ít nhất 8 ký tự"),
  rememberMe: z.boolean().optional(),
});

export const registerSchema = z
  .object({
    email: z.string().email("Email không hợp lệ"),
    fullName: z.string().min(3, "Họ tên phải có ít nhất 3 ký tự"),
    studentId: z.string().optional(),
    password: z
      .string()
      .min(8, "Mật khẩu phải có ít nhất 8 ký tự")
      .regex(/[A-Z]/, "Mật khẩu phải chứa ít nhất 1 chữ hoa")
      .regex(/[a-z]/, "Mật khẩu phải chứa ít nhất 1 chữ thường")
      .regex(/[0-9]/, "Mật khẩu phải chứa ít nhất 1 số")
      .regex(/[^A-Za-z0-9]/, "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Mật khẩu xác nhận không khớp",
    path: ["confirmPassword"],
  });

export const forgotPasswordSchema = z.object({
  email: z.string().email("Email không hợp lệ"),
});

export const resetPasswordSchema = z
  .object({
    password: z
      .string()
      .min(8, "Mật khẩu phải có ít nhất 8 ký tự")
      .regex(/[A-Z]/, "Mật khẩu phải chứa ít nhất 1 chữ hoa")
      .regex(/[a-z]/, "Mật khẩu phải chứa ít nhất 1 chữ thường")
      .regex(/[0-9]/, "Mật khẩu phải chứa ít nhất 1 số")
      .regex(/[^A-Za-z0-9]/, "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Mật khẩu xác nhận không khớp",
    path: ["confirmPassword"],
  });

export type LoginInput = z.infer<typeof loginSchema>;
export type RegisterInput = z.infer<typeof registerSchema>;
export type ForgotPasswordInput = z.infer<typeof forgotPasswordSchema>;
export type ResetPasswordInput = z.infer<typeof resetPasswordSchema>;
```

### 6. Login Page

**File**: `src/app/[locale]/(auth)/login/page.tsx`

```tsx
"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useTranslations } from "next-intl";
import { useLogin } from "@/hooks/auth/useLogin";
import { loginSchema, type LoginInput } from "@/lib/validations/auth.schema";
import { Link } from "@/lib/i18n/routing";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Loader2 } from "lucide-react";

export default function LoginPage() {
  const t = useTranslations("auth");
  const { mutate: login, isPending } = useLogin();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = (data: LoginInput) => {
    login(data);
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/50 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <CardTitle className="text-2xl font-bold">{t("login")}</CardTitle>
          <CardDescription>{t("loginDescription")}</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">{t("email")}</Label>
              <Input
                id="email"
                type="email"
                placeholder="student@unihub.edu.vn"
                {...register("email")}
                disabled={isPending}
              />
              {errors.email && (
                <p className="text-sm text-destructive">
                  {errors.email.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="password">{t("password")}</Label>
                <Link
                  href="/forgot-password"
                  className="text-sm text-primary hover:underline"
                >
                  {t("forgotPassword")}
                </Link>
              </div>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                {...register("password")}
                disabled={isPending}
              />
              {errors.password && (
                <p className="text-sm text-destructive">
                  {errors.password.message}
                </p>
              )}
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox id="rememberMe" {...register("rememberMe")} />
              <Label htmlFor="rememberMe" className="text-sm font-normal">
                {t("rememberMe")}
              </Label>
            </div>

            <Button type="submit" className="w-full" disabled={isPending}>
              {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {t("login")}
            </Button>
          </form>

          <div className="mt-4 text-center text-sm">
            {t("noAccount")}{" "}
            <Link href="/register" className="text-primary hover:underline">
              {t("register")}
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
```

### 7. Protected Route Wrapper

**File**: `src/components/features/auth/ProtectedRoute.tsx`

```tsx
"use client";

import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuthStore } from "@/stores/auth.store";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
}

export function ProtectedRoute({
  children,
  requiredRoles,
}: ProtectedRouteProps) {
  const router = useRouter();
  const pathname = usePathname();
  const { isAuthenticated, user } = useAuthStore();

  useEffect(() => {
    if (!isAuthenticated) {
      router.push(`/login?redirect=${encodeURIComponent(pathname)}`);
      return;
    }

    if (requiredRoles && user) {
      const hasRequiredRole = requiredRoles.some((role) =>
        user.roles.includes(role),
      );
      if (!hasRequiredRole) {
        router.push("/unauthorized");
      }
    }
  }, [isAuthenticated, user, requiredRoles, router, pathname]);

  if (!isAuthenticated) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <LoadingSpinner />
      </div>
    );
  }

  if (requiredRoles && user) {
    const hasRequiredRole = requiredRoles.some((role) =>
      user.roles.includes(role),
    );
    if (!hasRequiredRole) {
      return (
        <div className="flex min-h-screen items-center justify-center">
          <LoadingSpinner />
        </div>
      );
    }
  }

  return <>{children}</>;
}
```

### 8. Auth Layout

**File**: `src/app/[locale]/(auth)/layout.tsx`

```tsx
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Authentication - UniHub",
};

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-gradient-to-br from-primary/5 via-background to-secondary/5">
      {children}
    </div>
  );
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Login page working with email/password validation
- [ ] Register page with strong password requirements
- [ ] Forgot password sends email successfully
- [ ] Reset password with token from email works
- [ ] JWT tokens stored securely (localStorage/cookies)
- [ ] Access token refresh on 401 response (automatic)
- [ ] Logout clears all auth state
- [ ] Protected route redirects unauthenticated users
- [ ] Role-based access control working (Admin, Moderator, Student)
- [ ] Auth state persists across page refreshes
- [ ] Form validation shows clear error messages (in vi/en)
- [ ] Loading states during auth operations
- [ ] Toast notifications for success/error

---

## 🧪 TESTING CHECKLIST

- [ ] Login with valid credentials → redirects to home
- [ ] Login with invalid credentials → shows error
- [ ] Register with all valid fields → creates account
- [ ] Register with weak password → shows validation errors
- [ ] Forgot password with valid email → shows success message
- [ ] Reset password with valid token → updates password
- [ ] Access protected page without auth → redirects to login
- [ ] Access admin page without admin role → redirects to unauthorized
- [ ] Token refresh on 401 → automatically retries request
- [ ] Logout → clears state and redirects to login

---

_Last Updated: 2026-02-10_
