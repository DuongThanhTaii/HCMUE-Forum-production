# FE-19: Vercel Deployment

| Property | Value |
|---|---|
| **ID** | FE-19 |
| **Branch** | `feature/FE-19-deploy` |
| **Commit** | `chore(fe): configure Vercel deployment and environment variables` |
| **Priority** | Low |
| **Estimate** | 3h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-18 |

---

## Objective

Deploy frontend lên Vercel. Setup production environment variables, preview deployments cho develop branch.

---

## Vercel Configuration

### `vercel.json`

```json
{
  "framework": "nextjs",
  "buildCommand": "pnpm build",
  "installCommand": "pnpm install",
  "outputDirectory": ".next",
  "regions": ["sin1"],
  "headers": [
    {
      "source": "/(.*)",
      "headers": [
        { "key": "X-Frame-Options", "value": "DENY" },
        { "key": "X-Content-Type-Options", "value": "nosniff" },
        { "key": "Referrer-Policy", "value": "strict-origin-when-cross-origin" },
        { "key": "Permissions-Policy", "value": "camera=(), microphone=(), geolocation=()" }
      ]
    },
    {
      "source": "/sw.js",
      "headers": [
        { "key": "Cache-Control", "value": "public, max-age=0, must-revalidate" }
      ]
    }
  ]
}
```

---

## Environment Variables (Vercel Dashboard)

| Variable | Production | Preview |
|---|---|---|
| `NEXT_PUBLIC_API_URL` | `https://api.unihub.hcmue.edu.vn` | `https://api-staging.unihub...` |
| `NEXT_PUBLIC_SIGNALR_URL` | `https://api.unihub.hcmue.edu.vn/hubs` | staging |
| `NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME` | `unihub` | same |
| `NEXT_PUBLIC_CLOUDINARY_UPLOAD_PRESET` | `unihub_docs` | same |
| `NEXT_PUBLIC_APP_URL` | `https://unihub.hcmue.edu.vn` | preview URL |
| `NEXT_PUBLIC_ENABLE_PWA` | `true` | `false` |

---

## GitHub Actions CI (`.github/workflows/frontend-ci.yml`)

Verify existing CI workflow covers:
- [ ] `pnpm install`
- [ ] `pnpm tsc --noEmit` (type check)
- [ ] `pnpm lint`
- [ ] `pnpm build`

---

## Deployment Steps

1. Connect GitHub repo to Vercel
2. Set root directory: `frontend`
3. Configure environment variables in Vercel dashboard
4. Set Production branch: `main`
5. Set Preview branch: `develop`
6. Trigger first deploy

---

## Acceptance Criteria

- [ ] `pnpm build` thành công trong CI
- [ ] Production deployment live tại domain
- [ ] Preview deployment tự động cho mỗi PR
- [ ] Environment variables đã set
- [ ] Security headers configured
- [ ] PWA enabled trên production
- [ ] CORS headers từ BE cho phép production domain
