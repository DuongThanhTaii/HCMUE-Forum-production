# 🏗️ PHASE 0: FOUNDATION SETUP

> **Thiết lập môi trường phát triển, cấu trúc project, và CI/CD pipeline**

---

## 📋 PHASE INFO

| Property          | Value            |
| ----------------- | ---------------- |
| **Phase**         | 0                |
| **Name**          | Foundation Setup |
| **Status**        | 🔵 IN_PROGRESS   |
| **Progress**      | 4/8 tasks        |
| **Est. Duration** | 1 week           |
| **Dependencies**  | None             |

---

## 🎯 OBJECTIVES

- [ ] Tạo solution structure theo DDD pattern
- [ ] Setup tất cả projects trong solution
- [ ] Configure Docker cho local development
- [ ] Setup frontend với Next.js + Shadcn/ui
- [ ] Initialize Git repository với branching strategy
- [ ] Setup CI/CD pipeline
- [ ] Configure database connections
- [ ] Setup deployment on Railway

---

## 📝 TASKS

### TASK-001: Create Solution Structure

| Property         | Value                                 |
| ---------------- | ------------------------------------- |
| **ID**           | TASK-001                              |
| **Status**       | ✅ COMPLETED                          |
| **Priority**     | 🔴 Critical                           |
| **Estimate**     | 2 hours                               |
| **Branch**       | `feature/TASK-001-solution-structure` |
| **Dependencies** | None                                  |

**Description:**
Tạo cấu trúc thư mục và solution file cho .NET project theo DDD pattern.

**Acceptance Criteria:**

- [x] Solution file `UniHub.sln` created
- [x] Folder structure matches architecture docs
- [x] All project folders created (empty)
- [x] `Directory.Build.props` configured
- [x] `Directory.Packages.props` configured (central package management)

**Expected Output:**

```
HCMUE-Forum/
├── src/
│   ├── UniHub.API/
│   ├── Modules/
│   │   ├── Identity/
│   │   ├── Forum/
│   │   ├── Learning/
│   │   ├── Chat/
│   │   ├── Career/
│   │   ├── Notification/
│   │   └── AI/
│   └── Shared/
│       ├── UniHub.SharedKernel/
│       ├── UniHub.Infrastructure/
│       └── UniHub.Contracts/
├── tests/
├── docs/
├── Directory.Build.props
├── Directory.Packages.props
└── UniHub.sln
```

**Commit Message:**

```
feat(infra): create solution structure for DDD architecture

- Add UniHub.sln solution file
- Create folder structure for modules
- Add Directory.Build.props for common settings
- Add Directory.Packages.props for central package management

Refs: TASK-001
```

---

### TASK-002: Setup .NET 10 Projects

| Property         | Value                              |
| ---------------- | ---------------------------------- |
| **ID**           | TASK-002                           |
| **Status**       | ✅ COMPLETED                       |
| **Priority**     | 🔴 Critical                        |
| **Estimate**     | 3 hours                            |
| **Branch**       | `feature/TASK-002-dotnet-projects` |
| **Dependencies** | TASK-001                           |

**Description:**
Tạo tất cả các .NET projects với proper references.

**Acceptance Criteria:**

- [x] UniHub.API project created (ASP.NET Core Web API)
- [x] All module projects created (Domain, Application, Infrastructure, Presentation)
- [x] SharedKernel, Infrastructure, Contracts projects created
- [x] Project references configured correctly
- [x] Solution compiles without errors

**Projects to Create:**

```
# API
src/UniHub.API/UniHub.API.csproj

# Shared
src/Shared/UniHub.SharedKernel/UniHub.SharedKernel.csproj
src/Shared/UniHub.Infrastructure/UniHub.Infrastructure.csproj
src/Shared/UniHub.Contracts/UniHub.Contracts.csproj

# Identity Module
src/Modules/Identity/UniHub.Identity.Domain/UniHub.Identity.Domain.csproj
src/Modules/Identity/UniHub.Identity.Application/UniHub.Identity.Application.csproj
src/Modules/Identity/UniHub.Identity.Infrastructure/UniHub.Identity.Infrastructure.csproj
src/Modules/Identity/UniHub.Identity.Presentation/UniHub.Identity.Presentation.csproj

# Forum Module (same pattern)
# Learning Module (same pattern)
# Chat Module (same pattern)
# Career Module (same pattern)
# Notification Module (same pattern)
# AI Module (same pattern)

# Tests
tests/UniHub.UnitTests/UniHub.UnitTests.csproj
tests/UniHub.IntegrationTests/UniHub.IntegrationTests.csproj
tests/UniHub.ArchitectureTests/UniHub.ArchitectureTests.csproj
```

**Commit Message:**

```
feat(infra): setup all .NET 10 projects

- Create API project
- Create shared projects (SharedKernel, Infrastructure, Contracts)
- Create module projects for Identity, Forum, Learning, Chat, Career, Notification, AI
- Create test projects
- Configure project references

Refs: TASK-002
```

---

### TASK-003: Configure Docker for Local Development

| Property         | Value                           |
| ---------------- | ------------------------------- |
| **ID**           | TASK-003                        |
| **Status**       | ✅ COMPLETED                    |
| **Priority**     | 🟡 Medium                       |
| **Estimate**     | 2 hours                         |
| **Branch**       | `feature/TASK-003-docker-setup` |
| **Dependencies** | TASK-002                        |

**Description:**
Tạo Docker configuration cho local development với PostgreSQL, MongoDB, Redis.

**Acceptance Criteria:**

- [x] `docker-compose.yml` created
- [x] `docker-compose.override.yml` for development
- [x] PostgreSQL container configured
- [x] MongoDB container configured
- [x] Redis container configured
- [x] Containers start successfully with `docker-compose up`

**Files to Create:**

```yaml
# docker-compose.yml
version: "3.8"

services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_USER: unihub
      POSTGRES_PASSWORD: unihub_dev
      POSTGRES_DB: unihub
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  mongodb:
    image: mongo:7
    environment:
      MONGO_INITDB_ROOT_USERNAME: unihub
      MONGO_INITDB_ROOT_PASSWORD: unihub_dev
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  postgres_data:
  mongo_data:
  redis_data:
```

**Commit Message:**

```
feat(docker): add docker-compose for local development

- Add PostgreSQL 16 container
- Add MongoDB 7 container
- Add Redis 7 container
- Configure volumes for data persistence

Refs: TASK-003
```

---

### TASK-004: Create Next.js Frontend Project

| Property         | Value                           |
| ---------------- | ------------------------------- |
| **ID**           | TASK-004                        |
| **Status**       | ✅ COMPLETED                    |
| **Priority**     | 🔴 Critical                     |
| **Estimate**     | 3 hours                         |
| **Branch**       | `feature/TASK-004-nextjs-setup` |
| **Dependencies** | TASK-001                        |

**Description:**
Setup Next.js 14 project với App Router, TypeScript, Tailwind CSS, và Shadcn/ui.

**Acceptance Criteria:**

- [x] Next.js 14 project created with App Router
- [x] TypeScript configured
- [x] Tailwind CSS configured
- [x] Shadcn/ui initialized
- [x] Basic components installed (Button, Input, Card)
- [x] Folder structure created
- [x] PWA manifest added
- [x] App runs successfully on localhost

**Commands to Run:**

```bash
# Create Next.js project
npx create-next-app@latest frontend --typescript --tailwind --eslint --app --src-dir

# Initialize Shadcn/ui
npx shadcn-ui@latest init

# Add basic components
npx shadcn-ui@latest add button input card form dialog toast
```

**Folder Structure:**

```
frontend/
├── src/
│   ├── app/
│   │   ├── layout.tsx
│   │   ├── page.tsx
│   │   └── globals.css
│   ├── components/
│   │   ├── ui/              # Shadcn components
│   │   ├── features/        # Feature components
│   │   └── shared/          # Shared components
│   ├── lib/
│   │   └── utils.ts
│   ├── hooks/
│   ├── stores/
│   ├── types/
│   └── styles/
├── public/
│   ├── manifest.json        # PWA manifest
│   └── icons/
├── package.json
├── tsconfig.json
├── tailwind.config.ts
├── next.config.js
└── components.json          # Shadcn config
```

**Commit Message:**

```
feat(frontend): setup Next.js 14 with Shadcn/ui

- Initialize Next.js 14 with App Router
- Configure TypeScript and Tailwind CSS
- Initialize Shadcn/ui with basic components
- Create folder structure
- Add PWA manifest

Refs: TASK-004
```

---

### TASK-005: Setup Git Repository + Branching

| Property         | Value                                  |
| ---------------- | -------------------------------------- |
| **ID**           | TASK-005                               |
| **Status**       | ✅ COMPLETED                           |
| **Priority**     | 🔴 Critical                            |
| **Estimate**     | 1 hour                                 |
| **Branch**       | `main` (initial), then `develop`       |
| **Dependencies** | TASK-001, TASK-002, TASK-003, TASK-004 |

**Description:**
Initialize Git repository, push to GitHub, setup branching strategy.

**Acceptance Criteria:**

- [x] `.gitignore` created
- [x] Initial commit on `main`
- [x] Remote added (GitHub)
- [x] Pushed to GitHub
- [x] `develop` branch created
- [x] Branch protection rules set (optional)
- [x] PR template added

**Commands to Run:**

```bash
# Initialize git
git init

# Add remote
git remote add origin https://github.com/DuongThanhTaii/HCMUE-Forum.git

# Initial commit
git add .
git commit -m "chore: initial project setup"

# Push to main
git branch -M main
git push -u origin main

# Create develop branch
git checkout -b develop
git push -u origin develop
```

**Files to Create:**

- `.gitignore`
- `.github/PULL_REQUEST_TEMPLATE.md`

**Commit Message:**

```
chore: initial project setup with Git workflow

- Add .gitignore for .NET and Node.js
- Add PR template
- Setup main and develop branches

Refs: TASK-005
```

---

### TASK-006: Configure CI/CD (GitHub Actions)

| Property         | Value                         |
| ---------------- | ----------------------------- |
| **ID**           | TASK-006                      |
| **Status**       | ✅ COMPLETED                  |
| **Priority**     | 🟡 Medium                     |
| **Estimate**     | 2 hours                       |
| **Branch**       | `feature/TASK-006-cicd-setup` |
| **Dependencies** | TASK-005                      |

**Description:**
Setup GitHub Actions cho CI/CD pipeline.

**Acceptance Criteria:**

- [x] Build workflow for .NET
- [x] Build workflow for Next.js
- [x] Run tests on PR
- [x] Lint checks
- [x] Workflows pass on push

**Files to Create:**

```yaml
# .github/workflows/backend-ci.yml
name: Backend CI

on:
  push:
    branches: [main, develop]
    paths:
      - "src/**"
      - "tests/**"
  pull_request:
    branches: [main, develop]
    paths:
      - "src/**"
      - "tests/**"

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.0.x"
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

```yaml
# .github/workflows/frontend-ci.yml
name: Frontend CI

on:
  push:
    branches: [main, develop]
    paths:
      - "frontend/**"
  pull_request:
    branches: [main, develop]
    paths:
      - "frontend/**"

jobs:
  build:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: frontend
    steps:
      - uses: actions/checkout@v4
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: "20"
          cache: "npm"
          cache-dependency-path: frontend/package-lock.json
      - name: Install
        run: npm ci
      - name: Lint
        run: npm run lint
      - name: Build
        run: npm run build
```

**Commit Message:**

```
ci(github): add CI workflows for backend and frontend

- Add backend-ci.yml for .NET build and test
- Add frontend-ci.yml for Next.js build and lint
- Configure path filters for efficient CI

Refs: TASK-006
```

---

### TASK-007: Setup Database Connections

| Property         | Value                             |
| ---------------- | --------------------------------- |
| **ID**           | TASK-007                          |
| **Status**       | ✅ COMPLETED                      |
| **Priority**     | 🔴 Critical                       |
| **Estimate**     | 2 hours                           |
| **Branch**       | `feature/TASK-007-db-connections` |
| **Dependencies** | TASK-002                          |

**Description:**
Configure connection strings cho Neon.tech (PostgreSQL), MongoDB Atlas, và Upstash (Redis).

**Acceptance Criteria:**

- [x] `appsettings.json` template created
- [x] `appsettings.Development.json` with local connections
- [x] Environment variables documented
- [x] Connection test endpoint created
- [x] User secrets configured for sensitive data

**Files to Create:**

```json
// src/UniHub.API/appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "PostgreSQL": "",
    "MongoDB": "",
    "Redis": ""
  },
  "AllowedHosts": "*"
}
```

```json
// src/UniHub.API/appsettings.Development.json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=unihub;Username=unihub;Password=unihub_dev",
    "MongoDB": "mongodb://unihub:unihub_dev@localhost:27017",
    "Redis": "localhost:6379"
  }
}
```

**Environment Variables (Production):**

```
CONNECTIONSTRINGS__POSTGRESQL=<neon.tech connection string>
CONNECTIONSTRINGS__MONGODB=<mongodb atlas connection string>
CONNECTIONSTRINGS__REDIS=<upstash connection string>
```

**Commit Message:**

```
feat(infra): configure database connections

- Add appsettings.json template
- Add appsettings.Development.json for local dev
- Document environment variables for production
- Add connection string configuration

Refs: TASK-007
```

---

### TASK-008: Setup Railway Deployment

| Property         | Value                            |
| ---------------- | -------------------------------- |
| **ID**           | TASK-008                         |
| **Status**       | ✅ COMPLETED                     |
| **Priority**     | 🟡 Medium                        |
| **Estimate**     | 2 hours                          |
| **Branch**       | `feature/TASK-008-railway-setup` |
| **Dependencies** | TASK-006, TASK-007               |

**Description:**
Configure Railway deployment cho backend.

**Acceptance Criteria:**

- [x] `Dockerfile` created for API
- [x] `railway.toml` configured
- [x] Health check endpoint created
- [x] Deployment documentation written
- [x] Test deployment successful

**Files to Create:**

```dockerfile
# src/UniHub.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/UniHub.API/UniHub.API.csproj", "src/UniHub.API/"]
# Copy other project files...
RUN dotnet restore "src/UniHub.API/UniHub.API.csproj"
COPY . .
WORKDIR "/src/src/UniHub.API"
RUN dotnet build "UniHub.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UniHub.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UniHub.API.dll"]
```

```toml
# railway.toml
[build]
builder = "dockerfile"
dockerfilePath = "src/UniHub.API/Dockerfile"

[deploy]
healthcheckPath = "/health"
healthcheckTimeout = 100
restartPolicyType = "on_failure"
restartPolicyMaxRetries = 3
```

**Commit Message:**

```
feat(deploy): configure Railway deployment

- Add Dockerfile for API
- Add railway.toml configuration
- Add health check endpoint
- Document deployment process

Refs: TASK-008
```

---

## 🔗 DEPENDENCIES GRAPH

```
TASK-001 (Solution Structure)
    │
    ├──► TASK-002 (Setup .NET Projects)
    │        │
    │        ├──► TASK-003 (Docker Setup)
    │        │
    │        └──► TASK-007 (DB Connections)
    │
    └──► TASK-004 (Next.js Setup)

TASK-001 + TASK-002 + TASK-003 + TASK-004
    │
    └──► TASK-005 (Git Setup)
              │
              └──► TASK-006 (CI/CD)
                        │
                        └──► TASK-008 (Railway)
```

---

## ✅ COMPLETION CHECKLIST

- [x] TASK-001: Create Solution Structure
- [x] TASK-002: Setup .NET 10 Projects
- [x] TASK-003: Configure Docker
- [x] TASK-004: Create Next.js Frontend
- [x] TASK-005: Setup Git + Branching
- [x] TASK-006: Configure CI/CD
- [x] TASK-007: Setup DB Connections
- [x] TASK-008: Setup Railway

---

## 📝 NOTES

- Tất cả tasks phải follow coding conventions trong `RULES.md`
- Mỗi task hoàn thành phải có PR merge vào `develop`
- Update `STATUS.md` sau mỗi task
- Commit messages theo Conventional Commits

---

_Last Updated: 2026-02-04_
