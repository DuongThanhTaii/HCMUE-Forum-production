# TASK-105: Main Layout & Navigation

> **App layout with navbar, sidebar, footer, breadcrumbs**

---

## 📋 TASK INFO

| Property         | Value                          |
| ---------------- | ------------------------------ |
| **Task ID**      | TASK-105                       |
| **Module**       | Layout & Navigation            |
| **Status**       | ✅ COMPLETED                   |
| **Priority**     | 🔴 Critical                    |
| **Estimate**     | 5 hours                        |
| **Actual**       | 3.5 hours                      |
| **Completed**    | 2026-02-10                     |
| **Branch**       | `feature/TASK-105-main-layout` |
| **Dependencies** | TASK-104 (Auth)                |

---

## 🎯 OBJECTIVES

- Build responsive navbar with GAIA navbar-menu component
- Create collapsible sidebar navigation
- Implement mobile hamburger menu
- Add footer with links
- Create breadcrumbs component
- Setup dark mode toggle
- Add notification badge and user dropdown

---

## 📁 FILES TO CREATE

### 1. Main Layout

**File**: `src/app/[locale]/(main)/layout.tsx`

```tsx
import { Navbar } from "@/components/shared/layouts/Navbar";
import { Sidebar } from "@/components/shared/layouts/Sidebar";
import { Footer } from "@/components/shared/layouts/Footer";

export default function MainLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen flex-col">
      <Navbar />
      <div className="flex flex-1">
        <Sidebar />
        <main className="flex-1 overflow-auto">
          <div className="container mx-auto p-4 md:p-6 lg:p-8">{children}</div>
        </main>
      </div>
      <Footer />
    </div>
  );
}
```

### 2. Navbar Component (Uses GAIA UI)

**File**: `src/components/shared/layouts/Navbar.tsx`

```tsx
"use client";

import { useState } from "react";
import { useAuth } from "@/hooks/auth/useAuth";
import { useTranslations } from "next-intl";
import { Link } from "@/lib/i18n/routing";
import { NavbarMenu } from "@/components/ui/navbar-menu"; // GAIA UI
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { LanguageSwitcher } from "@/components/shared/LanguageSwitcher";
import { ThemeToggle } from "@/components/shared/ThemeToggle";
import { NotificationDropdown } from "@/components/features/notification/NotificationDropdown";
import { Menu, X, User, Settings, LogOut } from "lucide-react";

export function Navbar() {
  const { user, isAuthenticated, logout } = useAuth();
  const t = useTranslations("nav");
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  return (
    <nav className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        {/* Logo */}
        <Link href="/" className="flex items-center space-x-2">
          <div className="h-8 w-8 rounded-lg bg-primary" />
          <span className="text-xl font-bold">UniHub</span>
        </Link>

        {/* Desktop Navigation */}
        <NavbarMenu className="hidden md:flex">
          <Link href="/" className="nav-link">
            {t("home")}
          </Link>
          <Link href="/forum" className="nav-link">
            {t("forum")}
          </Link>
          <Link href="/learning" className="nav-link">
            {t("learning")}
          </Link>
          <Link href="/chat" className="nav-link">
            {t("chat")}
          </Link>
          <Link href="/career" className="nav-link">
            {t("career")}
          </Link>
        </NavbarMenu>

        {/* Right Actions */}
        <div className="flex items-center space-x-2">
          <ThemeToggle />
          <LanguageSwitcher />

          {isAuthenticated ? (
            <>
              <NotificationDropdown />
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button
                    variant="ghost"
                    className="relative h-10 w-10 rounded-full"
                  >
                    <Avatar>
                      <AvatarImage src={user?.avatar} alt={user?.fullName} />
                      <AvatarFallback>
                        {user?.fullName.charAt(0).toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <DropdownMenuLabel>
                    <div className="flex flex-col space-y-1">
                      <p className="text-sm font-medium">{user?.fullName}</p>
                      <p className="text-xs text-muted-foreground">
                        {user?.email}
                      </p>
                    </div>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem asChild>
                    <Link href={`/profile/${user?.id}`}>
                      <User className="mr-2 h-4 w-4" />
                      {t("profile")}
                    </Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem asChild>
                    <Link href="/settings">
                      <Settings className="mr-2 h-4 w-4" />
                      {t("settings")}
                    </Link>
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={logout}>
                    <LogOut className="mr-2 h-4 w-4" />
                    {t("logout")}
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </>
          ) : (
            <div className="hidden md:flex md:space-x-2">
              <Button variant="ghost" asChild>
                <Link href="/login">{t("login")}</Link>
              </Button>
              <Button asChild>
                <Link href="/register">{t("register")}</Link>
              </Button>
            </div>
          )}

          {/* Mobile Menu Toggle */}
          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          >
            {mobileMenuOpen ? <X /> : <Menu />}
          </Button>
        </div>
      </div>

      {/* Mobile Menu */}
      {mobileMenuOpen && (
        <div className="border-t md:hidden">
          <div className="container mx-auto space-y-1 px-4 py-4">
            <Link href="/" className="block rounded px-3 py-2 hover:bg-accent">
              {t("home")}
            </Link>
            <Link
              href="/forum"
              className="block rounded px-3 py-2 hover:bg-accent"
            >
              {t("forum")}
            </Link>
            <Link
              href="/learning"
              className="block rounded px-3 py-2 hover:bg-accent"
            >
              {t("learning")}
            </Link>
            <Link
              href="/chat"
              className="block rounded px-3 py-2 hover:bg-accent"
            >
              {t("chat")}
            </Link>
            <Link
              href="/career"
              className="block rounded px-3 py-2 hover:bg-accent"
            >
              {t("career")}
            </Link>
            {!isAuthenticated && (
              <>
                <div className="h-px bg-border" />
                <Link
                  href="/login"
                  className="block rounded px-3 py-2 hover:bg-accent"
                >
                  {t("login")}
                </Link>
                <Link
                  href="/register"
                  className="block rounded px-3 py-2 hover:bg-accent"
                >
                  {t("register")}
                </Link>
              </>
            )}
          </div>
        </div>
      )}
    </nav>
  );
}
```

### 3. Sidebar Component

**File**: `src/components/shared/layouts/Sidebar.tsx`

```tsx
"use client";

import { useState } from "react";
import { usePathname } from "next/navigation";
import { Link } from "@/lib/i18n/routing";
import { useTranslations } from "next-intl";
import { cn } from "@/lib/utils/cn";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Home,
  MessageSquare,
  BookOpen,
  MessageCircle,
  Briefcase,
  ChevronLeft,
  ChevronRight,
} from "lucide-react";

const menuItems = [
  { icon: Home, label: "home", href: "/" },
  { icon: MessageSquare, label: "forum", href: "/forum" },
  { icon: BookOpen, label: "learning", href: "/learning" },
  { icon: MessageCircle, label: "chat", href: "/chat" },
  { icon: Briefcase, label: "career", href: "/career" },
];

export function Sidebar() {
  const [collapsed, setCollapsed] = useState(false);
  const pathname = usePathname();
  const t = useTranslations("nav");

  return (
    <aside
      className={cn(
        "hidden border-r bg-muted/10 transition-all duration-300 md:block",
        collapsed ? "w-16" : "w-64",
      )}
    >
      <div className="flex h-full flex-col">
        <ScrollArea className="flex-1 px-3 py-4">
          <nav className="space-y-2">
            {menuItems.map((item) => {
              const Icon = item.icon;
              const isActive =
                pathname === item.href || pathname.startsWith(`${item.href}/`);

              return (
                <Link key={item.href} href={item.href}>
                  <Button
                    variant={isActive ? "secondary" : "ghost"}
                    className={cn(
                      "w-full",
                      collapsed ? "justify-center px-2" : "justify-start",
                    )}
                  >
                    <Icon className={cn("h-5 w-5", !collapsed && "mr-3")} />
                    {!collapsed && <span>{t(item.label)}</span>}
                  </Button>
                </Link>
              );
            })}
          </nav>
        </ScrollArea>

        <div className="border-t p-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setCollapsed(!collapsed)}
            className="w-full"
          >
            {collapsed ? <ChevronRight /> : <ChevronLeft />}
          </Button>
        </div>
      </div>
    </aside>
  );
}
```

### 4. Footer Component

**File**: `src/components/shared/layouts/Footer.tsx`

```tsx
import { Link } from "@/lib/i18n/routing";
import { useTranslations } from "next-intl";

export function Footer() {
  const t = useTranslations("footer");
  const currentYear = new Date().getFullYear();

  return (
    <footer className="border-t bg-muted/30">
      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 gap-8 md:grid-cols-4">
          <div>
            <h3 className="mb-4 text-lg font-semibold">UniHub</h3>
            <p className="text-sm text-muted-foreground">
              Nền tảng học tập và kết nối sinh viên
            </p>
          </div>

          <div>
            <h4 className="mb-4 text-sm font-semibold">Sản phẩm</h4>
            <ul className="space-y-2 text-sm">
              <li>
                <Link href="/forum" className="hover:underline">
                  Diễn đàn
                </Link>
              </li>
              <li>
                <Link href="/learning" className="hover:underline">
                  Học tập
                </Link>
              </li>
              <li>
                <Link href="/chat" className="hover:underline">
                  Trò chuyện
                </Link>
              </li>
              <li>
                <Link href="/career" className="hover:underline">
                  Nghề nghiệp
                </Link>
              </li>
            </ul>
          </div>

          <div>
            <h4 className="mb-4 text-sm font-semibold">Hỗ trợ</h4>
            <ul className="space-y-2 text-sm">
              <li>
                <Link href="/help" className="hover:underline">
                  Trợ giúp
                </Link>
              </li>
              <li>
                <Link href="/about" className="hover:underline">
                  Về chúng tôi
                </Link>
              </li>
              <li>
                <Link href="/contact" className="hover:underline">
                  Liên hệ
                </Link>
              </li>
            </ul>
          </div>

          <div>
            <h4 className="mb-4 text-sm font-semibold">Pháp lý</h4>
            <ul className="space-y-2 text-sm">
              <li>
                <Link href="/privacy" className="hover:underline">
                  Chính sách
                </Link>
              </li>
              <li>
                <Link href="/terms" className="hover:underline">
                  Điều khoản
                </Link>
              </li>
            </ul>
          </div>
        </div>

        <div className="mt-8 border-t pt-8 text-center text-sm text-muted-foreground">
          © {currentYear} UniHub. All rights reserved.
        </div>
      </div>
    </footer>
  );
}
```

### 5. Theme Toggle

**File**: `src/components/shared/ThemeToggle.tsx`

```tsx
"use client";

import { useTheme } from "next-themes";
import { Button } from "@/components/ui/button";
import { Moon, Sun } from "lucide-react";

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();

  return (
    <Button
      variant="ghost"
      size="icon"
      onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
    >
      <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
      <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
      <span className="sr-only">Toggle theme</span>
    </Button>
  );
}
```

### 6. Breadcrumbs Component

**File**: `src/components/shared/layouts/Breadcrumbs.tsx`

```tsx
"use client";

import { usePathname } from "next/navigation";
import { Link } from "@/lib/i18n/routing";
import { ChevronRight, Home } from "lucide-react";
import { Fragment } from "react";

export function Breadcrumbs() {
  const pathname = usePathname();
  const segments = pathname.split("/").filter(Boolean);

  // Remove locale from segments
  const pathSegments =
    segments[0] === "vi" || segments[0] === "en" ? segments.slice(1) : segments;

  if (pathSegments.length === 0) return null;

  return (
    <nav className="mb-4 flex items-center space-x-2 text-sm text-muted-foreground">
      <Link href="/" className="hover:text-foreground">
        <Home className="h-4 w-4" />
      </Link>
      {pathSegments.map((segment, index) => {
        const href = `/${pathSegments.slice(0, index + 1).join("/")}`;
        const isLast = index === pathSegments.length - 1;
        const label = segment.charAt(0).toUpperCase() + segment.slice(1);

        return (
          <Fragment key={href}>
            <ChevronRight className="h-4 w-4" />
            {isLast ? (
              <span className="font-medium text-foreground">{label}</span>
            ) : (
              <Link href={href} className="hover:text-foreground">
                {label}
              </Link>
            )}
          </Fragment>
        );
      })}
    </nav>
  );
}
```

### 7. Root Layout with Providers

**File**: `src/app/[locale]/layout.tsx`

```tsx
import { NextIntlClientProvider } from "next-intl";
import { getMessages } from "next-intl/server";
import { ThemeProvider } from "next-themes";
import { QueryProvider } from "@/app/providers";
import { Toaster } from "sonner";
import { Inter } from "next/font/google";
import "../globals.css";

const inter = Inter({ subsets: ["latin", "vietnamese"] });

export default async function RootLayout({
  children,
  params: { locale },
}: {
  children: React.ReactNode;
  params: { locale: string };
}) {
  const messages = await getMessages();

  return (
    <html lang={locale} suppressHydrationWarning>
      <body className={inter.className}>
        <ThemeProvider attribute="class" defaultTheme="system" enableSystem>
          <NextIntlClientProvider messages={messages}>
            <QueryProvider>
              {children}
              <Toaster position="top-right" />
            </QueryProvider>
          </NextIntlClientProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
```

**File**: `src/app/providers.tsx`

```tsx
"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useState } from "react";

export function QueryProvider({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000, // 1 minute
            refetchOnWindowFocus: false,
          },
        },
      }),
  );

  return (
    <QueryClientProvider client={queryClient}>
      {children}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Navbar responsive (desktop + mobile)
- [ ] GAIA navbar-menu component integrated
- [ ] Sidebar collapsible on desktop
- [ ] Mobile hamburger menu working
- [ ] User dropdown with profile/settings/logout
- [ ] Dark mode toggle functional
- [ ] Language switcher working (vi/en)
- [ ] Notification badge shows unread count
- [ ] Footer with links
- [ ] Breadcrumbs show current path
- [ ] All navigation links functional
- [ ] Active route highlighted in sidebar
- [ ] Layout adapts to different screen sizes

---

_Last Updated: 2026-02-10_
