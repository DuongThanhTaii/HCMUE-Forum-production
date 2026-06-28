# UniHub - HCMUE Forum

> 🎓 Nền tảng cộng đồng đại học thông minh cho Trường ĐHSP TPHCM

[![Backend CI](https://github.com/DuongThanhTaii/HCMUE-Forum/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/DuongThanhTaii/HCMUE-Forum/actions)
[![Frontend CI](https://github.com/DuongThanhTaii/HCMUE-Forum/actions/workflows/frontend-ci.yml/badge.svg)](https://github.com/DuongThanhTaii/HCMUE-Forum/actions)

---

## 📋 Mục Lục

- [Giới Thiệu](#-giới-thiệu)
- [Tính Năng](#-tính-năng)
- [Tech Stack](#-tech-stack)
- [Kiến Trúc](#-kiến-trúc)
- [Cài Đặt](#-cài-đặt)
- [Development](#-development)
- [Deployment](#-deployment)
- [Documentation](#-documentation)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Giới Thiệu

UniHub là nền tảng cộng đồng đại học toàn diện, kết nối sinh viên, giảng viên, phòng ban và nhà tuyển dụng trong một hệ sinh thái thống nhất.

### Mục Tiêu

- 💬 **Forum**: Diễn đàn thảo luận, Q&A, confession
- 📚 **Learning Hub**: Quản lý tài liệu học tập với hệ thống duyệt
- 💼 **Career Hub**: Kết nối việc làm, thực tập
- 💬 **Chat**: Nhắn tin real-time, nhóm, kênh cộng đồng
- 🤖 **AI Assistant**: Chatbot hỗ trợ, kiểm duyệt nội dung
- 🏛️ **Official Accounts**: Tài khoản phòng ban với badge xác minh

---

## ✨ Tính Năng

### Core Features

| Module           | Tính Năng                                                     |
| ---------------- | ------------------------------------------------------------- |
| **Identity**     | Đăng ký, đăng nhập, JWT, phân quyền động, Official Badge      |
| **Forum**        | Posts, Comments, Votes, Tags, Search, Bookmark, Report        |
| **Learning**     | Upload tài liệu, Approval workflow, Rating, Download tracking |
| **Chat**         | 1:1, Group, Channels, File sharing, Real-time                 |
| **Career**       | Job posting, Company profiles, Applications, Matching         |
| **Notification** | Push, Email, In-app, Preferences                              |
| **AI**           | Chatbot, Content moderation, Summarization                    |

### Technical Highlights

- 🏗️ **DDD Architecture**: Domain-Driven Design với Modular Monolith
- 📨 **CQRS**: Tách biệt Command và Query
- 📅 **Event Sourcing**: Cho Document Approval workflow
- ⚡ **Real-time**: SignalR với Redis backplane
- 🔐 **Security**: JWT, Refresh Token rotation, RBAC động

---

## 🛠️ Tech Stack

### Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- MediatR + FluentValidation
- SignalR
- Serilog

### Frontend

- Next.js 14 (App Router)
- TypeScript
- Tailwind CSS
- Shadcn/ui
- Zustand + TanStack Query

### Database

- PostgreSQL (Neon.tech)
- MongoDB (Atlas)
- Redis (Upstash)

### Deployment

- Railway (Backend)
- Vercel (Frontend)

---

## 🏗️ Kiến Trúc

```
┌─────────────────────────────────────────────────────────────┐
│                      PRESENTATION                           │
│                    (API Controllers)                        │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      APPLICATION                            │
│            (Commands, Queries, Handlers)                    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                        DOMAIN                               │
│        (Entities, Value Objects, Domain Events)             │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE                           │
│          (Repositories, External Services)                  │
└─────────────────────────────────────────────────────────────┘
```

### Modules

```
src/Modules/
├── Identity/        # Authentication, Authorization
├── Forum/           # Posts, Comments, Votes
├── Learning/        # Documents, Courses, Approval
├── Chat/            # Messaging, Channels
├── Career/          # Jobs, Companies
├── Notification/    # Push, Email, In-app
└── AI/              # Chatbot, Moderation
```

---

## 📦 Cài Đặt

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (optional, for local databases)

### Clone Repository

```bash
git clone https://github.com/DuongThanhTaii/HCMUE-Forum.git
cd HCMUE-Forum
```

### Backend Setup

```bash
# Restore packages
dotnet restore

# Run with local databases (Docker)
docker-compose up -d

# Run API
cd src/UniHub.API
dotnet run
```

### Frontend Setup

Primary UI is **React + Vite** under `frontend/`. The former Next.js app lives in `frontend-next/` only for migration comparison.

```bash
cd frontend

# Install dependencies
npm install

# Run development server
npm run dev

# Lint + i18n parity + production build (recommended before PR)
npm run verify
```

---

## 🚀 Development

### Branch Strategy

```
main        # Production-ready code
develop     # Integration branch
feature/*   # New features
bugfix/*    # Bug fixes
hotfix/*    # Critical fixes
```

### Commit Convention

```
feat(module): add new feature
fix(module): fix bug
docs(module): update documentation
refactor(module): code refactoring
test(module): add tests
chore(module): maintenance
```

### Useful Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run specific project
dotnet run --project src/UniHub.API

# Frontend verify (lint + i18n + build)
cd frontend && npm run verify
```

---

## 🌐 Deployment

### Backend (Railway)

1. Connect GitHub repository
2. Set environment variables
3. Deploy

### Frontend (Vercel)

1. Import project from GitHub
2. Configure build settings
3. Deploy

### Environment Variables

```env
# Backend
CONNECTIONSTRINGS__POSTGRESQL=<neon_connection_string>
CONNECTIONSTRINGS__MONGODB=<atlas_connection_string>
CONNECTIONSTRINGS__REDIS=<upstash_connection_string>
JWT__SECRET=<your_jwt_secret>
JWT__ISSUER=<your_issuer>
JWT__AUDIENCE=<your_audience>

# Frontend
NEXT_PUBLIC_API_URL=<api_url>
```

---

## 📖 Documentation

- [Agent Context](docs/AGENT_CONTEXT.md) - Project overview for AI agents
- [Rules](docs/RULES.md) - Coding conventions
- [Architecture](docs/ARCHITECTURE.md) - System architecture
- [Git Workflow](docs/GIT_WORKFLOW.md) - Git conventions
- [Tasks](docs/tasks/STATUS.md) - Project progress

---

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'feat: add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Dương Thành Tài**

- GitHub: [@DuongThanhTaii](https://github.com/DuongThanhTaii)

---

⭐ Star this repo if you find it helpful!
