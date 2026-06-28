# 🚀 Railway Deployment Guide

> Step-by-step guide để deploy UniHub backend lên Railway

---

## 📋 Prerequisites

- ✅ Railway account: [railway.app](https://railway.app)
- ✅ GitHub repository connected
- ✅ Database services setup (Neon.tech PostgreSQL, MongoDB Atlas, Upstash Redis)

---

## 🔧 Setup Railway Project

### 1. Create New Project

```bash
# Install Railway CLI (optional)
npm install -g @railway/cli

# Login to Railway
railway login

# Link to existing project or create new
railway init
```

Or use Railway Dashboard:
1. Go to [railway.app/new](https://railway.app/new)
2. Select "Deploy from GitHub repo"
3. Choose `DuongThanhTaii/HCMUE-Forum`
4. Railway will auto-detect the `railway.toml`

### 2. Configure Environment Variables

Add these variables in Railway dashboard (Settings → Variables):

```bash
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080

# PostgreSQL (Neon.tech)
CONNECTIONSTRINGS__POSTGRESQL=Host=<neon-host>;Port=5432;Database=<db>;Username=<user>;Password=<pass>;SSL Mode=Require

# MongoDB (Atlas)
CONNECTIONSTRINGS__MONGODB=mongodb+srv://<user>:<pass>@<cluster>.mongodb.net/<db>?retryWrites=true&w=majority

# Redis (Upstash)
CONNECTIONSTRINGS__REDIS=<upstash-redis-url>
```

### 3. Configure Build Settings

Railway should auto-detect from `railway.toml`:

```toml
[build]
builder = "dockerfile"
dockerfilePath = "src/UniHub.API/Dockerfile"

[deploy]
healthcheckPath = "/health"
healthcheckTimeout = 100
restartPolicyType = "on_failure"
restartPolicyMaxRetries = 3
```

---

## 🏗️ Build & Deploy

### First Deployment

1. **Push to GitHub**
   ```bash
   git push origin main
   ```

2. **Railway auto-deploys** from `main` branch
   - Build time: ~3-5 minutes
   - Docker build with .NET 10
   - Health check at `/health`

3. **Check Deployment**
   - View logs in Railway dashboard
   - Check build status
   - Verify health check passes

### Subsequent Deployments

Every push to `main` triggers auto-deployment:

```bash
git checkout main
git merge develop
git push origin main
```

---

## 🔍 Verify Deployment

### 1. Health Check

```bash
curl https://your-app.railway.app/health
```

Expected response:
```json
{
  "status": "Healthy"
}
```

### 2. Connection Test

```bash
curl https://your-app.railway.app/health/connections
```

Expected response:
```json
{
  "status": "Healthy",
  "timestamp": "2026-02-15T10:30:00Z",
  "connectionsConfigured": {
    "postgreSQL": true,
    "mongoDB": true,
    "redis": true
  }
}
```

### 3. OpenAPI/Swagger

```bash
curl https://your-app.railway.app/openapi/v1.json
```

---

## 📊 Monitoring

### Railway Dashboard

- **Deployments**: View deployment history
- **Metrics**: CPU, Memory, Network usage
- **Logs**: Real-time application logs
- **Settings**: Environment variables, domains

### View Logs

```bash
# Using Railway CLI
railway logs

# Or in dashboard: Deployments → View Logs
```

### Metrics to Monitor

- ✅ Response time
- ✅ Error rate
- ✅ Memory usage
- ✅ CPU usage
- ✅ Request rate

---

## 🌐 Custom Domain

### Add Custom Domain

1. Go to Settings → Domains
2. Click "Add Domain"
3. Enter your domain: `api.unihub.com`
4. Add DNS records:
   ```
   Type: CNAME
   Name: api
   Value: your-app.railway.app
   ```

### SSL Certificate

Railway automatically provisions SSL certificates via Let's Encrypt.

---

## 🔄 CI/CD Integration

### GitHub Actions + Railway

Our `.github/workflows/backend-ci.yml` runs tests before Railway deploys:

```yaml
name: Backend CI

on:
  push:
    branches: [main, develop]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
      - name: Test
        run: dotnet test
```

**Flow:**
1. Push to `develop` → GitHub Actions runs tests
2. Merge PR to `main` → GitHub Actions runs tests
3. Tests pass → Railway auto-deploys

---

## 🐛 Troubleshooting

### Build Fails

**Problem:** Docker build fails

**Solutions:**
1. Check Dockerfile syntax
2. Verify all project references exist
3. Check Railway build logs
4. Test Docker build locally:
   ```bash
   docker build -f src/UniHub.API/Dockerfile -t unihub-api .
   ```

### Health Check Fails

**Problem:** Deployment fails health check

**Solutions:**
1. Verify `/health` endpoint works
2. Check health check timeout (default: 100s)
3. Increase timeout in `railway.toml`
4. Check application logs for startup errors

### Environment Variables

**Problem:** App can't connect to databases

**Solutions:**
1. Verify all env vars are set in Railway
2. Check connection string format
3. Test connections with `/health/connections`
4. Verify database services are accessible from Railway

### High Memory Usage

**Problem:** App crashes with OOM

**Solutions:**
1. Upgrade Railway plan
2. Optimize database queries
3. Add memory profiling
4. Check for memory leaks

---

## 💰 Cost Estimation

### Railway Pricing (2026)

- **Hobby Plan**: $5/month
  - 512MB RAM
  - Shared CPU
  - Good for development/testing

- **Pro Plan**: $20/month
  - 8GB RAM
  - 8 vCPU
  - Production-ready

### External Services

- **Neon.tech**: Free tier (1GB)
- **MongoDB Atlas**: Free tier (512MB)
- **Upstash Redis**: Free tier (10K commands/day)

**Total for development: $5-10/month**

---

## 📚 Resources

- [Railway Documentation](https://docs.railway.app)
- [Railway CLI](https://docs.railway.app/develop/cli)
- [Dockerfile Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Environment Variables Guide](./ENVIRONMENT_VARIABLES.md)

---

## 🔐 Security Checklist

- [ ] All secrets stored in Railway environment variables
- [ ] No secrets in code or Dockerfile
- [ ] SSL/TLS enabled (automatic with Railway)
- [ ] Database connections use SSL
- [ ] CORS configured properly
- [ ] Rate limiting enabled
- [ ] Health check endpoint doesn't expose sensitive data

---

## 📝 Deployment Checklist

Before deploying to production:

- [ ] All tests pass
- [ ] Database migrations applied
- [ ] Environment variables configured
- [ ] Health check endpoint working
- [ ] Logs configured properly
- [ ] Error monitoring setup
- [ ] Custom domain configured (optional)
- [ ] SSL certificate verified
- [ ] Performance tested
- [ ] Security audit completed

---

_Last Updated: 2026-02-15_
