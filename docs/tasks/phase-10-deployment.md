# TASK-115: Vercel Deployment & Production Setup

> **Deploy to Vercel, environment variables, domains, CI/CD, monitoring**

---

## 📋 TASK INFO

| Property         | Value                                 |
| ---------------- | ------------------------------------- |
| **Task ID**      | TASK-115                              |
| **Module**       | Deployment                            |
| **Status**       | ⬜ NOT_STARTED                        |
| **Priority**     | 🔴 Critical                           |
| **Estimate**     | 4 hours                               |
| **Branch**       | N/A (deploy from main)                |
| **Dependencies** | ALL (final deployment after all work) |

---

## 🎯 OBJECTIVES

- Deploy frontend to Vercel
- Configure environment variables
- Setup custom domain
- Configure preview deployments
- Setup monitoring & analytics
- Configure build optimization
- Add edge functions (if needed)

---

## 📁 KEY FILES

### 1. Vercel Configuration

**File**: `vercel.json`

```json
{
  "buildCommand": "npm run build",
  "devCommand": "npm run dev",
  "installCommand": "npm install",
  "framework": "nextjs",
  "regions": ["sin1"],
  "headers": [
    {
      "source": "/api/(.*)",
      "headers": [
        { "key": "Access-Control-Allow-Credentials", "value": "true" },
        { "key": "Access-Control-Allow-Origin", "value": "*" },
        {
          "key": "Access-Control-Allow-Methods",
          "value": "GET,OPTIONS,PATCH,DELETE,POST,PUT"
        },
        {
          "key": "Access-Control-Allow-Headers",
          "value": "X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Content-Length, Content-MD5, Content-Type, Date, X-Api-Version, Authorization"
        }
      ]
    }
  ],
  "rewrites": [
    {
      "source": "/api/:path*",
      "destination": "https://api.unihub.example/api/:path*"
    }
  ],
  "redirects": [
    {
      "source": "/home",
      "destination": "/",
      "permanent": true
    }
  ]
}
```

### 2. Environment Variables (.env.example)

**File**: `.env.example`

```bash
# API Configuration
NEXT_PUBLIC_API_URL=https://api.unihub.example
NEXT_PUBLIC_SIGNALR_HUB_URL=https://api.unihub.example/hubs

# Cloudinary Configuration
NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME=your_cloud_name
NEXT_PUBLIC_CLOUDINARY_UPLOAD_PRESET=your_upload_preset
CLOUDINARY_API_KEY=your_api_key
CLOUDINARY_API_SECRET=your_api_secret

# App Configuration
NEXT_PUBLIC_APP_URL=https://unihub.example
NEXT_PUBLIC_APP_NAME=UniHub

# Analytics (Optional)
NEXT_PUBLIC_GA_ID=G-XXXXXXXXXX
NEXT_PUBLIC_SENTRY_DSN=https://xxx@sentry.io/xxx

# Feature Flags
NEXT_PUBLIC_FEATURE_AI_CHAT=true
NEXT_PUBLIC_FEATURE_PWA=true
```

### 3. Vercel Deployment Script

**File**: `scripts/deploy.sh`

```bash
#!/bin/bash

echo "🚀 Starting deployment to Vercel..."

# Check if vercel CLI is installed
if ! command -v vercel &> /dev/null; then
    echo "Installing Vercel CLI..."
    npm i -g vercel
fi

# Set environment
ENVIRONMENT=$1
if [ -z "$ENVIRONMENT" ]; then
    ENVIRONMENT="production"
fi

echo "Deploying to: $ENVIRONMENT"

# Build and deploy
if [ "$ENVIRONMENT" == "production" ]; then
    vercel --prod --token=$VERCEL_TOKEN
else
    vercel --token=$VERCEL_TOKEN
fi

echo "✅ Deployment complete!"
```

### 4. GitHub Actions Deployment Workflow

**File**: `.github/workflows/deploy.yml`

```yaml
name: Deploy to Vercel

on:
  push:
    branches:
      - main
  pull_request:
    branches:
      - main

env:
  VERCEL_ORG_ID: ${{ secrets.VERCEL_ORG_ID }}
  VERCEL_PROJECT_ID: ${{ secrets.VERCEL_PROJECT_ID }}

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: "20"
          cache: "npm"

      - name: Install Vercel CLI
        run: npm install --global vercel@latest

      - name: Pull Vercel Environment Information
        run: vercel pull --yes --environment=${{ github.ref == 'refs/heads/main' && 'production' || 'preview' }} --token=${{ secrets.VERCEL_TOKEN }}

      - name: Build Project Artifacts
        run: vercel build ${{ github.ref == 'refs/heads/main' && '--prod' || '' }} --token=${{ secrets.VERCEL_TOKEN }}

      - name: Deploy Project Artifacts to Vercel
        id: deploy
        run: |
          url=$(vercel deploy --prebuilt ${{ github.ref == 'refs/heads/main' && '--prod' || '' }} --token=${{ secrets.VERCEL_TOKEN }})
          echo "url=$url" >> $GITHUB_OUTPUT

      - name: Comment PR with deployment URL
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: `✅ Preview deployment ready!\n\n🔗 **URL:** ${{ steps.deploy.outputs.url }}`
            })
```

### 5. Vercel Ignored Build Step

**File**: `scripts/vercel-ignore-build.sh`

```bash
#!/bin/bash

# Skip build for specific branches or conditions
if [[ "$VERCEL_GIT_COMMIT_REF" == "draft/"* ]] ; then
  echo "🛑 Branch starts with 'draft/' - skipping build"
  exit 0
fi

# Check if only docs were changed
if git diff HEAD^ HEAD --quiet -- './docs/**' ; then
  echo "📝 Only docs changed - proceeding with build"
  exit 1
else
  echo "✅ Code changes detected - building"
  exit 1
fi
```

### 6. Next.js Production Config

**File**: `next.config.js` (production optimizations)

```javascript
const withPWA = require("next-pwa")({
  dest: "public",
  disable: process.env.NODE_ENV === "development",
});

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,

  // Image optimization
  images: {
    domains: ["res.cloudinary.com", "api.unihub.example"],
    formats: ["image/avif", "image/webp"],
  },

  // Compression
  compress: true,

  // Bundle analyzer (enable for debugging)
  // webpack: (config, { isServer }) => {
  //   if (!isServer) {
  //     const { BundleAnalyzerPlugin } = require('webpack-bundle-analyzer');
  //     config.plugins.push(
  //       new BundleAnalyzerPlugin({
  //         analyzerMode: 'static',
  //         openAnalyzer: false,
  //       })
  //     );
  //   }
  //   return config;
  // },

  // Headers for security
  async headers() {
    return [
      {
        source: "/(.*)",
        headers: [
          {
            key: "X-Content-Type-Options",
            value: "nosniff",
          },
          {
            key: "X-Frame-Options",
            value: "DENY",
          },
          {
            key: "X-XSS-Protection",
            value: "1; mode=block",
          },
          {
            key: "Referrer-Policy",
            value: "strict-origin-when-cross-origin",
          },
        ],
      },
    ];
  },

  // Redirects
  async redirects() {
    return [
      {
        source: "/admin",
        destination: "/admin/dashboard",
        permanent: true,
      },
    ];
  },
};

module.exports = withPWA(nextConfig);
```

### 7. Monitoring & Error Tracking Setup

**File**: `src/lib/monitoring/sentry.ts`

```typescript
import * as Sentry from "@sentry/nextjs";

export function initializeSentry() {
  if (process.env.NEXT_PUBLIC_SENTRY_DSN) {
    Sentry.init({
      dsn: process.env.NEXT_PUBLIC_SENTRY_DSN,
      environment: process.env.NODE_ENV,
      tracesSampleRate: 1.0,
      beforeSend(event) {
        // Filter out sensitive information
        if (event.request) {
          delete event.request.cookies;
        }
        return event;
      },
    });
  }
}
```

**File**: `src/app/layout.tsx` (add Sentry)

```tsx
import { initializeSentry } from "@/lib/monitoring/sentry";

// Initialize monitoring
if (typeof window !== "undefined") {
  initializeSentry();
}
```

### 8. Analytics Setup

**File**: `src/lib/analytics/google-analytics.ts`

```typescript
export const GA_TRACKING_ID = process.env.NEXT_PUBLIC_GA_ID;

// https://developers.google.com/analytics/devguides/collection/gtagjs/pages
export const pageview = (url: string) => {
  if (typeof window !== "undefined" && GA_TRACKING_ID) {
    window.gtag("config", GA_TRACKING_ID, {
      page_path: url,
    });
  }
};

// https://developers.google.com/analytics/devguides/collection/gtagjs/events
export const event = ({
  action,
  category,
  label,
  value,
}: {
  action: string;
  category: string;
  label: string;
  value?: number;
}) => {
  if (typeof window !== "undefined" && GA_TRACKING_ID) {
    window.gtag("event", action, {
      event_category: category,
      event_label: label,
      value: value,
    });
  }
};
```

---

## 🚀 DEPLOYMENT CHECKLIST

### Pre-Deployment

- [ ] All tests passing (`npm run test`)
- [ ] No linting errors (`npm run lint`)
- [ ] Build succeeds locally (`npm run build`)
- [ ] Environment variables documented
- [ ] API endpoints tested in staging
- [ ] Browser compatibility tested
- [ ] Mobile responsiveness tested
- [ ] Lighthouse score > 90

### Vercel Setup

- [ ] Create Vercel account/project
- [ ] Link GitHub repository
- [ ] Configure environment variables in Vercel dashboard
- [ ] Set up custom domain (if applicable)
- [ ] Configure CORS for production API
- [ ] Enable automatic deployments from main branch
- [ ] Setup preview deployments for PRs

### Post-Deployment

- [ ] Verify production deployment works
- [ ] Test all critical user flows
- [ ] Check PWA installation
- [ ] Verify SignalR connections work
- [ ] Test file uploads to Cloudinary
- [ ] Monitor error tracking (Sentry)
- [ ] Check analytics (Google Analytics)
- [ ] Test SEO meta tags
- [ ] Verify sitemap.xml accessible

### Monitoring

- [ ] Setup Sentry error tracking
- [ ] Configure Google Analytics
- [ ] Enable Vercel Analytics
- [ ] Setup uptime monitoring (UptimeRobot, Pingdom)
- [ ] Configure alerts for errors
- [ ] Monitor Core Web Vitals

---

## 🔑 REQUIRED SECRETS

Add these secrets to Vercel project settings or GitHub repository:

```
VERCEL_TOKEN=<your_vercel_token>
VERCEL_ORG_ID=<your_org_id>
VERCEL_PROJECT_ID=<your_project_id>

NEXT_PUBLIC_API_URL=https://api.unihub.example
NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME=<cloud_name>
NEXT_PUBLIC_CLOUDINARY_UPLOAD_PRESET=<preset>
CLOUDINARY_API_KEY=<api_key>
CLOUDINARY_API_SECRET=<api_secret>

NEXT_PUBLIC_GA_ID=<google_analytics_id>
NEXT_PUBLIC_SENTRY_DSN=<sentry_dsn>
```

---

## 📊 PERFORMANCE TARGETS

| Metric                    | Target  |
| ------------------------- | ------- |
| First Contentful Paint    | < 1.8s  |
| Largest Contentful Paint  | < 2.5s  |
| Time to Interactive       | < 3.8s  |
| Cumulative Layout Shift   | < 0.1   |
| First Input Delay         | < 100ms |
| Lighthouse Performance    | > 90    |
| Lighthouse SEO            | > 95    |
| Lighthouse Accessibility  | > 90    |
| Lighthouse Best Practices | > 95    |
| Lighthouse PWA            | 100     |

---

## 🌐 DOMAIN CONFIGURATION

### Custom Domain Setup

1. Add domain in Vercel: Project Settings → Domains
2. Configure DNS records:
   ```
   A     @       76.76.21.21
   CNAME www     cname.vercel-dns.com
   ```
3. Wait for DNS propagation (up to 48 hours)
4. Enable automatic HTTPS

### Subdomains

- `app.unihub.example` → Production
- `staging.unihub.example` → Staging environment
- `dev.unihub.example` → Development preview

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Frontend deployed to Vercel
- [ ] Production URL accessible
- [ ] All environment variables configured
- [ ] Custom domain setup (if applicable)
- [ ] HTTPS enabled
- [ ] Preview deployments for PRs
- [ ] CI/CD pipeline working
- [ ] Error tracking active
- [ ] Analytics tracking active
- [ ] Performance monitoring active
- [ ] Uptime monitoring configured
- [ ] Lighthouse score > 90
- [ ] SEO meta tags working
- [ ] PWA installable
- [ ] SignalR connections working in production

---

_Last Updated: 2026-02-10_
