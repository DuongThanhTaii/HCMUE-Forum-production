# 🔐 Environment Variables

> Configuration guide for UniHub environment variables

---

## 📝 Overview

This document lists all environment variables required for UniHub backend API.

## 🗄️ Database Connections

### PostgreSQL (Neon.tech)

```bash
CONNECTIONSTRINGS__POSTGRESQL="Host=<neon-host>;Port=5432;Database=<db-name>;Username=<username>;Password=<password>;SSL Mode=Require"
```

**Example:**
```bash
CONNECTIONSTRINGS__POSTGRESQL="Host=ep-cool-grass-123456.us-east-2.aws.neon.tech;Port=5432;Database=unihub_prod;Username=unihub_user;Password=your_secure_password;SSL Mode=Require"
```

**Local Development (appsettings.Development.json):**
```bash
Host=localhost;Port=5432;Database=unihub;Username=unihub;Password=unihub_dev
```

### MongoDB (MongoDB Atlas)

```bash
CONNECTIONSTRINGS__MONGODB="mongodb+srv://<username>:<password>@<cluster-url>/<database>?retryWrites=true&w=majority"
```

**Example:**
```bash
CONNECTIONSTRINGS__MONGODB="mongodb+srv://unihub_user:your_secure_password@cluster0.abc123.mongodb.net/unihub_prod?retryWrites=true&w=majority"
```

**Local Development (appsettings.Development.json):**
```bash
mongodb://unihub:unihub_dev@localhost:27017
```

### Redis (Upstash)

```bash
CONNECTIONSTRINGS__REDIS="<upstash-redis-url>"
```

**Example:**
```bash
CONNECTIONSTRINGS__REDIS="rediss://:your_token@flying-bat-12345.upstash.io:6379"
```

**Local Development (appsettings.Development.json):**
```bash
localhost:6379
```

---

## 🧠 Identity Permission Cache

Control dynamic authorization permission cache provider and behavior.

```bash
IDENTITY__PERMISSIONCACHE__PROVIDER=InMemory
IDENTITY__PERMISSIONCACHE__EXPIRATIONMINUTES=15
IDENTITY__PERMISSIONCACHE__REDISINSTANCENAME=UniHub:Identity
```

**Recommended values:**
- `InMemory` for local development and single-instance test runs.
- `Redis` for staging/production multi-instance deployments.

When using `Redis`, ensure `CONNECTIONSTRINGS__REDIS` is configured.

---

## 🧾 User Action Logging (Middleware)

Enable detailed per-request action trace for auditing user behavior.

```bash
OBSERVABILITY__USERACTIONLOGGING__ENABLED=true
OBSERVABILITY__USERACTIONLOGGING__PERSISTTOMONGO=true
OBSERVABILITY__USERACTIONLOGGING__MONGOCOLLECTIONNAME=user_action_logs
OBSERVABILITY__USERACTIONLOGGING__RETENTIONDAYS=90
OBSERVABILITY__USERACTIONLOGGING__DEFAULTQUERYPAGESIZE=100
OBSERVABILITY__USERACTIONLOGGING__MAXQUERYPAGESIZE=500
OBSERVABILITY__USERACTIONLOGGING__CORRELATIONHEADERNAME=X-Correlation-Id
OBSERVABILITY__USERACTIONLOGGING__EXCLUDEDPATHPREFIXES__0=/health
OBSERVABILITY__USERACTIONLOGGING__EXCLUDEDPATHPREFIXES__1=/openapi
OBSERVABILITY__USERACTIONLOGGING__EXCLUDEDPATHPREFIXES__2=/scalar
OBSERVABILITY__USERACTIONLOGGING__EXCLUDEDPATHPREFIXES__3=/hubs
```

Logged fields include:
- actor user id (`sub` / `nameidentifier` / anonymous)
- method, path, endpoint display name
- status code, duration, result
- trace id + correlation id
- remote IP + user agent

---

## 🚀 Deployment Platforms

### Railway

Set environment variables in Railway dashboard:

1. Go to your project
2. Click on "Variables" tab
3. Add the following variables:
   - `CONNECTIONSTRINGS__POSTGRESQL`
   - `CONNECTIONSTRINGS__MONGODB`
   - `CONNECTIONSTRINGS__REDIS`
   - `ASPNETCORE_ENVIRONMENT=Production`

### Vercel (Frontend)

```bash
NEXT_PUBLIC_API_URL=https://your-api-url.railway.app
```

---

## 🌱 High-Volume Seeding (Local)

Use these settings only for local performance testing datasets.

```bash
SEEDING__HIGHVOLUME__ENABLED=true
SEEDING__HIGHVOLUME__FORCERESEED=true
SEEDING__HIGHVOLUME__SKIPMIGRATIONS=true
SEEDING__HIGHVOLUME__TARGETTOTALROWS=500000
SEEDING__HIGHVOLUME__BATCHSIZE=5000
SEEDING__HIGHVOLUME__EXITAFTERSEEDING=true
```

Notes:
- `SEEDING__HIGHVOLUME__ENABLED`: bật chế độ seed dữ liệu lớn.
- `SEEDING__HIGHVOLUME__FORCERESEED`: xóa và seed lại toàn bộ bảng trong shared PostgreSQL schema.
- `SEEDING__HIGHVOLUME__SKIPMIGRATIONS`: bỏ qua `MigrateAsync()` trong lúc seed lớn nếu local DB đã có schema.
- `SEEDING__HIGHVOLUME__TARGETTOTALROWS`: tổng số bản ghi mục tiêu (hệ thống sẽ scale theo tỷ lệ, mặc định ~500k).
- `SEEDING__HIGHVOLUME__BATCHSIZE`: số record mỗi batch SQL (nên bắt đầu 2k-10k tùy máy).
- `SEEDING__HIGHVOLUME__EXITAFTERSEEDING`: seed xong thì app tự thoát thay vì chạy server API (tránh cảm giác bị treo khi chỉ muốn seed).

### Learning bulk (DEV — courses + tài liệu đã duyệt)

Dùng cho FE/API cần **nhiều** dòng trong `learning.courses` / `learning.documents` (phân trang, filter) **không** xóa toàn DB như HighVolume.

Trong `appsettings.Development.json` hoặc biến môi trường:

| Key | Ý nghĩa |
|-----|---------|
| `Seeding:Learning:Bulk:Enabled` | Bật seed nhiều course + document (mặc định `true` trong file dev mẫu). |
| `Seeding:Learning:Bulk:OnlyIfEmpty` | `true`: chỉ chạy khi **chưa có** document (tránh nhân đôi mỗi lần chạy API). Đặt `false` nếu muốn chạy lại (cần xóa dữ liệu learning trước để tránh trùng mã course). |
| `Seeding:Learning:Bulk:CoursesPerFaculty` | Số môn seed **mỗi khoa** (mặc định 10). |
| `Seeding:Learning:Bulk:DocumentsPerCourse` | Số tài liệu **mỗi môn** (mặc định 15). |
| `Seeding:Learning:Bulk:MaxDocuments` | Giới hạn tổng số document sinh ra (mặc định 8000). |
| `Seeding:Learning:Bulk:SaveBatchSize` | Kích thước batch `SaveChanges` (100–500). |

Biến môi trường (uppercase, `__`): ví dụ `SEEDING__LEARNING__BULK__ENABLED=true`.

---

## 🔒 User Secrets (Local Development)

For sensitive data during local development, use .NET User Secrets:

```bash
# Initialize user secrets
cd src/UniHub.API
dotnet user-secrets init

# Set connection strings
dotnet user-secrets set "ConnectionStrings:PostgreSQL" "your-local-connection"
dotnet user-secrets set "ConnectionStrings:MongoDB" "your-local-connection"
dotnet user-secrets set "ConnectionStrings:Redis" "your-local-connection"

# List all secrets
dotnet user-secrets list

# Clear all secrets
dotnet user-secrets clear
```

**Note:** User secrets override values in `appsettings.Development.json`

---

## ⚠️ Security Best Practices

1. **Never commit secrets** to Git
2. **Use different credentials** for each environment
3. **Rotate credentials** regularly
4. **Use strong passwords** (min 16 characters, mixed case, numbers, symbols)
5. **Enable 2FA** on cloud provider accounts
6. **Restrict database access** by IP when possible
7. **Use SSL/TLS** for all connections

---

## 📚 References

- [Neon.tech Connection Strings](https://neon.tech/docs/connect/connect-from-any-app)
- [MongoDB Atlas Connection Strings](https://www.mongodb.com/docs/atlas/driver-connection/)
- [Upstash Redis Connection](https://upstash.com/docs/redis/overall/getstarted)
- [.NET Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

---

## 🧪 Testing Connections

Test connection strings with the health check endpoint:

```bash
# Local
curl http://localhost:5000/health/connections

# Production
curl https://your-api-url.railway.app/health/connections
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
