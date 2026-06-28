# TASK-113: Progressive Web App (PWA) Features

> **Service worker, offline support, install prompt, app manifest**

---

## 📋 TASK INFO

| Property         | Value                         |
| ---------------- | ----------------------------- |
| **Task ID**      | TASK-113                      |
| **Module**       | PWA Features                  |
| **Status**       | ⬜ NOT_STARTED                |
| **Priority**     | 🟢 Low                        |
| **Estimate**     | 4 hours                       |
| **Branch**       | `feature/TASK-113-pwa-setup` |
| **Dependencies** | TASK-101                       |

---

## 🎯 OBJECTIVES

- Configure PWA with next-pwa
- Create app manifest
- Setup service worker
- Implement offline fallback
- Add install prompt
- Generate app icons
- Enable push notifications

---

## 📁 KEY FILES

### 1. Next.js PWA Configuration

**File**: `next.config.js`

```javascript
const withPWA = require('next-pwa')({
  dest: 'public',
  register: true,
  skipWaiting: true,
  disable: process.env.NODE_ENV === 'development',
  runtimeCaching: [
    {
      urlPattern: /^https:\/\/api\.unihub\.example\/.*$/,
      handler: 'NetworkFirst',
      options: {
        cacheName: 'api-cache',
        expiration: {
          maxEntries: 50,
          maxAgeSeconds: 300, // 5 minutes
        },
        networkTimeoutSeconds: 10,
      },
    },
    {
      urlPattern: /\.(?:png|jpg|jpeg|svg|gif|webp)$/,
      handler: 'CacheFirst',
      options: {
        cacheName: 'image-cache',
        expiration: {
          maxEntries: 100,
          maxAgeSeconds: 30 * 24 * 60 * 60, // 30 days
        },
      },
    },
    {
      urlPattern: /^https:\/\/fonts\.(?:googleapis|gstatic)\.com\/.*/,
      handler: 'CacheFirst',
      options: {
        cacheName: 'google-fonts',
        expiration: {
          maxEntries: 10,
          maxAgeSeconds: 365 * 24 * 60 * 60, // 1 year
        },
      },
    },
  ],
});

module.exports = withPWA({
  // ... existing Next.js config
  reactStrictMode: true,
  experimental: {
    optimizeCss: true,
  },
});
```

### 2. App Manifest

**File**: `public/manifest.json`

```json
{
  "name": "UniHub - Mạng xã hội sinh viên",
  "short_name": "UniHub",
  "description": "Nền tảng kết nối sinh viên, chia sẻ tài liệu và tìm việc làm",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#3b82f6",
  "orientation": "portrait-primary",
  "icons": [
    {
      "src": "/icons/icon-72x72.png",
      "sizes": "72x72",
      "type": "image/png",
      "purpose": "maskable any"
    },
    {
      "src": "/icons/icon-96x96.png",
      "sizes": "96x96",
      "type": "image/png",
      "purpose": "maskable any"
    },
    {
      "src": "/icons/icon-128x128.png",
      "sizes": "128x128",
      "type": "image/png",
      "purpose": "maskable any"
    },
    {
      "src": "/icons/icon-144x144.png",
      "sizes": "144x144",
      "type": "image/png",
      "purpose": "maskable any"
    },
    {
      "src": "/icons/icon-152x152.png",
      "sizes": "152x152",
      "type": "image/png",
      "purpose": "maskable any"
    },
    {
      "src": "/icons/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "maskable any"
    },
    {
      "src": "/icons/icon-384x384.png",
      "sizes": "384x384",
      "type": "image/png",
      "purpose": "maskable any"
    },
    {
      "src": "/icons/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "maskable any"
    }
  ],
  "categories": ["education", "social", "productivity"],
  "screenshots": [
    {
      "src": "/screenshots/desktop-1.png",
      "sizes": "1280x720",
      "type": "image/png",
      "form_factor": "wide"
    },
    {
      "src": "/screenshots/mobile-1.png",
      "sizes": "750x1334",
      "type": "image/png",
      "form_factor": "narrow"
    }
  ],
  "shortcuts": [
    {
      "name": "Diễn đàn",
      "short_name": "Forum",
      "description": "Xem bài viết mới nhất",
      "url": "/forum",
      "icons": [
        {
          "src": "/icons/forum-shortcut.png",
          "sizes": "96x96"
        }
      ]
    },
    {
      "name": "Tin nhắn",
      "short_name": "Chat",
      "description": "Mở hội thoại",
      "url": "/chat",
      "icons": [
        {
          "src": "/icons/chat-shortcut.png",
          "sizes": "96x96"
        }
      ]
    },
    {
      "name": "Tài liệu",
      "short_name": "Docs",
      "description": "Tìm tài liệu học tập",
      "url": "/learning/documents",
      "icons": [
        {
          "src": "/icons/docs-shortcut.png",
          "sizes": "96x96"
        }
      ]
    }
  ]
}
```

### 3. Root Layout with PWA Meta Tags

**File**: `src/app/layout.tsx` (add to existing)

```tsx
export const metadata = {
  manifest: '/manifest.json',
  appleWebApp: {
    capable: true,
    statusBarStyle: 'default',
    title: 'UniHub',
  },
  formatDetection: {
    telephone: false,
  },
  // ... other existing metadata
};

export default function RootLayout({ children }) {
  return (
    <html>
      <head>
        {/* PWA Meta Tags */}
        <meta name="application-name" content="UniHub" />
        <meta name="apple-mobile-web-app-capable" content="yes" />
        <meta name="apple-mobile-web-app-status-bar-style" content="default" />
        <meta name="apple-mobile-web-app-title" content="UniHub" />
        <meta name="format-detection" content="telephone=no" />
        <meta name="mobile-web-app-capable" content="yes" />
        <meta name="theme-color" content="#3b82f6" />

        {/* Apple Touch Icons */}
        <link rel="apple-touch-icon" href="/icons/icon-152x152.png" />
        <link rel="apple-touch-icon" sizes="152x152" href="/icons/icon-152x152.png" />
        <link rel="apple-touch-icon" sizes="180x180" href="/icons/icon-180x180.png" />
        <link rel="apple-touch-icon" sizes="167x167" href="/icons/icon-167x167.png" />

        {/* Splash Screens */}
        <link
          rel="apple-touch-startup-image"
          href="/splash/apple-splash-2048-2732.png"
          media="(device-width: 1024px) and (device-height: 1366px) and (-webkit-device-pixel-ratio: 2) and (orientation: portrait)"
        />
        <link
          rel="apple-touch-startup-image"
          href="/splash/apple-splash-1668-2388.png"
          media="(device-width: 834px) and (device-height: 1194px) and (-webkit-device-pixel-ratio: 2) and (orientation: portrait)"
        />
      </head>
      <body>{children}</body>
    </html>
  );
}
```

### 4. Install Prompt Component

**File**: `src/components/shared/InstallPrompt.tsx`

```tsx
'use client';

import { useEffect, useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Download, X } from 'lucide-react';

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

export function InstallPrompt() {
  const [deferredPrompt, setDeferredPrompt] = useState<BeforeInstallPromptEvent | null>(null);
  const [showPrompt, setShowPrompt] = useState(false);

  useEffect(() => {
    const handler = (e: Event) => {
      e.preventDefault();
      setDeferredPrompt(e as BeforeInstallPromptEvent);
      
      // Check if user dismissed before
      const dismissed = localStorage.getItem('pwa-install-dismissed');
      if (!dismissed) {
        setShowPrompt(true);
      }
    };

    window.addEventListener('beforeinstallprompt', handler);

    return () => {
      window.removeEventListener('beforeinstallprompt', handler);
    };
  }, []);

  const handleInstall = async () => {
    if (!deferredPrompt) return;

    deferredPrompt.prompt();
    const { outcome } = await deferredPrompt.userChoice;

    if (outcome === 'accepted') {
      console.log('PWA installed');
    }

    setDeferredPrompt(null);
    setShowPrompt(false);
  };

  const handleDismiss = () => {
    setShowPrompt(false);
    localStorage.setItem('pwa-install-dismissed', 'true');
  };

  if (!showPrompt) return null;

  return (
    <Card className="fixed bottom-4 left-4 right-4 z-50 md:left-auto md:right-4 md:w-96">
      <CardContent className="flex items-center gap-4 pt-6">
        <div className="flex-1">
          <h3 className="font-semibold">Cài đặt UniHub</h3>
          <p className="text-sm text-muted-foreground">
            Cài đặt ứng dụng để truy cập nhanh hơn
          </p>
        </div>
        <div className="flex gap-2">
          <Button size="sm" onClick={handleInstall}>
            <Download className="mr-1 h-3 w-3" />
            Cài đặt
          </Button>
          <Button size="sm" variant="ghost" onClick={handleDismiss}>
            <X className="h-4 w-4" />
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
```

### 5. Offline Fallback Page

**File**: `src/app/offline/page.tsx`

```tsx
export default function OfflinePage() {
  return (
    <div className="flex min-h-screen items-center justify-center p-4">
      <div className="text-center">
        <div className="mb-4 text-6xl">📡</div>
        <h1 className="mb-2 text-2xl font-bold">Không có kết nối</h1>
        <p className="text-muted-foreground">
          Vui lòng kiểm tra kết nối internet của bạn
        </p>
        <button
          onClick={() => window.location.reload()}
          className="mt-4 rounded-lg bg-primary px-4 py-2 text-white"
        >
          Thử lại
        </button>
      </div>
    </div>
  );
}
```

### 6. Custom Service Worker (Optional)

**File**: `public/sw.js`

```javascript
// Custom service worker logic
self.addEventListener('push', (event) => {
  const data = event.data?.json() ?? {};
  
  event.waitUntil(
    self.registration.showNotification(data.title, {
      body: data.body,
      icon: '/icons/icon-192x192.png',
      badge: '/icons/badge-72x72.png',
      tag: data.tag,
      data: data.url,
    })
  );
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  
  event.waitUntil(
    clients.openWindow(event.notification.data)
  );
});
```

### 7. Package.json Scripts

**File**: `package.json` (add scripts)

```json
{
  "scripts": {
    "dev": "next dev",
    "build": "next build",
    "start": "next start",
    "lint": "next lint",
    "pwa:generate-icons": "pwa-asset-generator public/logo.svg public/icons --padding '10%' --background '#ffffff'",
    "pwa:generate-splash": "pwa-asset-generator public/logo.svg public/splash --splash-only"
  }
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] PWA configuration with next-pwa
- [ ] App manifest with all required fields
- [ ] Service worker registration
- [ ] Offline fallback page
- [ ] Install prompt component
- [ ] App icons (all sizes: 72, 96, 128, 144, 152, 192, 384, 512)
- [ ] Apple touch icons
- [ ] Apple splash screens
- [ ] Runtime caching strategy
- [ ] Push notifications setup (optional)
- [ ] App shortcuts in manifest
- [ ] Screenshots for app stores
- [ ] Lighthouse PWA score 100
- [ ] Works offline (cached pages)
- [ ] Installable on mobile and desktop

---

## 🎨 ICON GENERATION

Use `pwa-asset-generator` to generate all required icons:

```bash
npx pwa-asset-generator public/logo.svg public/icons \
  --padding "10%" \
  --background "#ffffff" \
  --icon-only

npx pwa-asset-generator public/logo.svg public/splash \
  --padding "20%" \
  --background "#3b82f6" \
  --splash-only
```

---

_Last Updated: 2026-02-10_
