# 🤖 AGENT CONTEXT - UNIHUB PROJECT

> **Đọc file này đầu tiên** trước khi implement bất kỳ task nào.

---

## 📋 PROJECT OVERVIEW

| Property         | Value                                                                                               |
| ---------------- | --------------------------------------------------------------------------------------------------- |
| **Project Name** | UniHub - HCMUE Forum                                                                                |
| **Description**  | Nền tảng cộng đồng đại học thông minh với Forum, Career Hub, Learning Resources, Chat, AI Assistant |
| **Architecture** | DDD (Domain-Driven Design) + Modular Monolith + CQRS                                                |
| **Target Users** | Sinh viên, Giảng viên, Phòng ban, Nhà tuyển dụng - Trường ĐHSP TPHCM                                |

---

## 🛠️ TECH STACK

### Backend

| Technology            | Version | Purpose                     |
| --------------------- | ------- | --------------------------- |
| .NET                  | 10      | Main framework              |
| ASP.NET Core          | 10      | Web API                     |
| Entity Framework Core | 10      | ORM for PostgreSQL          |
| MongoDB.Driver        | Latest  | NoSQL for Chat, Documents   |
| MediatR               | Latest  | CQRS + Mediator pattern     |
| FluentValidation      | Latest  | Request validation          |
| SignalR               | Latest  | Real-time communication     |
| Serilog               | Latest  | Structured logging          |
| Redis (StackExchange) | Latest  | Caching + SignalR backplane |

### Frontend

| Technology       | Version | Purpose                      |
| ---------------- | ------- | ---------------------------- |
| Next.js          | 14+     | React framework (App Router) |
| TypeScript       | 5+      | Type safety                  |
| Tailwind CSS     | 3+      | Styling                      |
| Shadcn/ui        | Latest  | UI Component library         |
| Zustand          | Latest  | State management             |
| TanStack Query   | Latest  | Server state management      |
| Socket.io Client | Latest  | Real-time client             |

### Databases

| Database   | Provider  | Purpose                              |
| ---------- | --------- | ------------------------------------ |
| PostgreSQL | Neon.tech | Main relational data                 |
| MongoDB    | Atlas     | Chat messages, Documents metadata    |
| Redis      | Upstash   | Caching, Sessions, SignalR backplane |

### Deployment

| Service  | Platform                          | Purpose           |
| -------- | --------------------------------- | ----------------- |
| Backend  | Railway                           | .NET API hosting  |
| Frontend | Vercel                            | Next.js hosting   |
| Database | Neon.tech, MongoDB Atlas, Upstash | Managed databases |

### AI Providers (Rotation)

| Provider | Priority  | Fallback                              |
| -------- | --------- | ------------------------------------- |
| Groq     | Primary   | When quota exceeded → Gemini          |
| Gemini   | Secondary | When quota exceeded → Other free APIs |

---

## 📁 PROJECT STRUCTURE

```
HCMUE-Forum/
├── src/
│   ├── UniHub.API/                      # API Gateway (Entry Point)
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── Extensions/
│   │   └── Program.cs
│   │
│   ├── Modules/
│   │   ├── Identity/
│   │   │   ├── UniHub.Identity.Domain/
│   │   │   ├── UniHub.Identity.Application/
│   │   │   ├── UniHub.Identity.Infrastructure/
│   │   │   └── UniHub.Identity.Presentation/
│   │   │
│   │   ├── Forum/
│   │   │   ├── UniHub.Forum.Domain/
│   │   │   ├── UniHub.Forum.Application/
│   │   │   ├── UniHub.Forum.Infrastructure/
│   │   │   └── UniHub.Forum.Presentation/
│   │   │
│   │   ├── Learning/
│   │   │   └── ... (same structure)
│   │   │
│   │   ├── Chat/
│   │   │   └── ... (same structure)
│   │   │
│   │   ├── Career/
│   │   │   └── ... (same structure)
│   │   │
│   │   ├── Notification/
│   │   │   └── ... (same structure)
│   │   │
│   │   └── AI/
│   │       └── ... (same structure)
│   │
│   └── Shared/
│       ├── UniHub.SharedKernel/          # DDD base classes
│       ├── UniHub.Infrastructure/        # Cross-cutting concerns
│       └── UniHub.Contracts/             # Shared DTOs, Events
│
├── frontend/
│   ├── src/
│   │   ├── app/                          # Next.js App Router
│   │   ├── components/
│   │   │   ├── ui/                       # Shadcn components
│   │   │   └── features/                 # Feature components
│   │   ├── lib/
│   │   ├── hooks/
│   │   ├── stores/
│   │   ├── types/
│   │   └── styles/
│   ├── public/
│   └── package.json
│
├── tests/
│   ├── UniHub.UnitTests/
│   ├── UniHub.IntegrationTests/
│   └── UniHub.ArchitectureTests/
│
├── docs/
│   ├── AGENT_CONTEXT.md                  # This file
│   ├── RULES.md                          # Coding rules
│   ├── ARCHITECTURE.md                   # Architecture details
│   ├── GIT_WORKFLOW.md                   # Git conventions
│   └── tasks/
│       ├── STATUS.md                     # Overall status
│       └── phase-*.md                    # Phase details
│
├── docker-compose.yml
├── Directory.Build.props
├── Directory.Packages.props
├── .gitignore
├── README.md
└── UniHub.sln
```

---

## 🎯 BOUNDED CONTEXTS

| Context          | Type       | Database             | CQRS | Event Sourcing     |
| ---------------- | ---------- | -------------------- | ---- | ------------------ |
| **Identity**     | Core       | PostgreSQL           | ✅   | ❌                 |
| **Forum**        | Core       | PostgreSQL           | ✅   | ❌                 |
| **Learning**     | Core       | PostgreSQL + MongoDB | ✅   | ✅ (Approval only) |
| **Chat**         | Core       | MongoDB              | ✅   | ❌                 |
| **Career**       | Supporting | PostgreSQL           | ✅   | ❌                 |
| **Notification** | Supporting | PostgreSQL           | ✅   | ❌                 |
| **AI**           | Generic    | MongoDB (logs)       | ❌   | ❌                 |

---

## 📖 IMPORTANT FILES TO READ

Trước khi implement, agent **BẮT BUỘC** phải đọc:

1. `docs/RULES.md` - Coding conventions
2. `docs/ARCHITECTURE.md` - Architecture patterns
3. `docs/GIT_WORKFLOW.md` - Git branching strategy
4. `docs/tasks/STATUS.md` - Current progress
5. `docs/tasks/phase-X.md` - Current phase details

---

## ⚠️ CRITICAL RULES FOR AGENTS

1. **KHÔNG tự ý thay đổi architecture** đã define
2. **KHÔNG skip tests** - mỗi feature phải có unit test
3. **KHÔNG merge trực tiếp vào main** - phải qua PR
4. **PHẢI update task status** sau khi hoàn thành
5. **PHẢI follow naming conventions** trong RULES.md
6. **PHẢI commit theo Conventional Commits**

---

## 🔗 REPOSITORY

- **GitHub**: https://github.com/DuongThanhTaii/HCMUE-Forum.git
- **Main Branch**: `main`
- **Development Branch**: `develop`

---

_Last Updated: 2026-02-04_
