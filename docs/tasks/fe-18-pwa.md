# FE-18: PWA — Manifest + Service Worker

| Property | Value |
|---|---|
| **ID** | FE-18 |
| **Branch** | `feature/FE-18-pwa` |
| **Commit** | `chore(fe): configure PWA manifest and service worker` |
| **Priority** | Low |
| **Estimate** | 3h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-01 |

---

## Objective

Làm UniHub installable như native app trên mobile/desktop. Cache static assets, offline fallback page.

---

## Files

### `frontend/public/manifest.json`

```json
{
  "name": "UniHub - HCMUE Forum",
  "short_name": "UniHub",
  "description": "Cộng đồng sinh viên Trường ĐHSP TPHCM",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#124874",
  "theme_color": "#124874",
  "orientation": "portrait-primary",
  "icons": [
    { "src": "/icons/icon-72x72.png", "sizes": "72x72", "type": "image/png" },
    { "src": "/icons/icon-96x96.png", "sizes": "96x96", "type": "image/png" },
    { "src": "/icons/icon-128x128.png", "sizes": "128x128", "type": "image/png" },
    { "src": "/icons/icon-144x144.png", "sizes": "144x144", "type": "image/png" },
    { "src": "/icons/icon-152x152.png", "sizes": "152x152", "type": "image/png" },
    { "src": "/icons/icon-192x192.png", "sizes": "192x192", "type": "image/png", "purpose": "maskable" },
    { "src": "/icons/icon-384x384.png", "sizes": "384x384", "type": "image/png" },
    { "src": "/icons/icon-512x512.png", "sizes": "512x512", "type": "image/png", "purpose": "any maskable" }
  ],
  "categories": ["education", "social"],
  "lang": "vi"
}
```

### `next.config.ts` — next-pwa config

```ts
const withPWA = require('next-pwa')({
  dest: 'public',
  register: true,
  skipWaiting: true,
  disable: process.env.NODE_ENV === 'development',
  runtimeCaching: [
    {
      urlPattern: /^https:\/\/fonts\.googleapis\.com/,
      handler: 'CacheFirst',
      options: { cacheName: 'google-fonts', expiration: { maxEntries: 10, maxAgeSeconds: 365 * 24 * 60 * 60 } },
    },
    {
      urlPattern: /^https:\/\/api\./,
      handler: 'NetworkFirst',
      options: { cacheName: 'api-cache', expiration: { maxEntries: 50, maxAgeSeconds: 5 * 60 } },
    },
  ],
});
```

---

## Assets Required

Generate từ logo design system:
- `public/icons/icon-192x192.png` (maskable)
- `public/icons/icon-512x512.png`
- `public/favicon.ico`
- `public/apple-touch-icon.png` (180x180)
- `public/og-image.png` (1200x630) — cho Open Graph

---

## Acceptance Criteria

- [ ] Lighthouse PWA audit: Installable
- [ ] manifest.json valid
- [ ] Icons tất cả sizes có mặt
- [ ] Service worker register thành công
- [ ] App installable trên Chrome Android
- [ ] Offline fallback page hiện khi mất kết nối
- [ ] theme_color #124874 hiện trên mobile browser bar
