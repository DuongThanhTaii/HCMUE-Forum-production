# 🔀 GIT WORKFLOW & CONVENTIONS

> **Quy trình Git chuẩn enterprise cho UniHub project**

---

## 🌳 BRANCHING STRATEGY (Git Flow)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           GIT FLOW                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  main ──────●────────────────●────────────────●──────────────────────►      │
│             │                ▲                ▲                              │
│             │                │                │                              │
│             │         merge  │         merge  │                              │
│             │                │                │                              │
│  develop ───●────●────●──────●────●────●──────●────●────●──────────────►     │
│             │    │    │           │    │           │    │                   │
│             │    │    │           │    │           │    │                   │
│  feature/   │    ●────┘           │    │           │    │                   │
│  auth       │                     │    │           │    │                   │
│             │                     │    ●───────────┘    │                   │
│  feature/   │                     │                     │                   │
│  forum      │                     ●─────────────────────┘                   │
│             │                                                               │
│  hotfix/    └──●──► (direct to main + develop)                              │
│  critical                                                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📌 BRANCH NAMING CONVENTIONS

### Format

```
{type}/{ticket-id}-{short-description}
```

### Types

| Type        | Usage                     | Example                                |
| ----------- | ------------------------- | -------------------------------------- |
| `feature/`  | New feature               | `feature/TASK-001-user-authentication` |
| `bugfix/`   | Bug fix                   | `bugfix/TASK-045-login-validation`     |
| `hotfix/`   | Critical fix (production) | `hotfix/TASK-099-security-patch`       |
| `refactor/` | Code refactoring          | `refactor/TASK-023-clean-repositories` |
| `docs/`     | Documentation             | `docs/TASK-012-api-documentation`      |
| `test/`     | Testing                   | `test/TASK-056-unit-tests-forum`       |
| `chore/`    | Maintenance               | `chore/TASK-078-update-packages`       |

### Examples

```bash
# Good ✅
feature/TASK-001-user-registration
bugfix/TASK-023-fix-post-validation
hotfix/TASK-099-critical-security-fix
refactor/TASK-045-clean-architecture
docs/TASK-012-add-api-docs

# Bad ❌
new-feature
fix
my-branch
feature/add-login   # Missing ticket ID
```

---

## 📝 COMMIT MESSAGE CONVENTIONS (Conventional Commits)

### Format

```
{type}({scope}): {subject}

{body}

{footer}
```

### Types

| Type       | Description   | Example                                     |
| ---------- | ------------- | ------------------------------------------- |
| `feat`     | New feature   | `feat(auth): add user registration`         |
| `fix`      | Bug fix       | `fix(forum): resolve post validation error` |
| `docs`     | Documentation | `docs(readme): update installation guide`   |
| `style`    | Formatting    | `style(api): fix indentation`               |
| `refactor` | Refactoring   | `refactor(identity): extract token service` |
| `test`     | Tests         | `test(forum): add post creation tests`      |
| `chore`    | Maintenance   | `chore(deps): update packages`              |
| `perf`     | Performance   | `perf(query): optimize post listing`        |
| `ci`       | CI/CD         | `ci(github): add build workflow`            |
| `build`    | Build system  | `build(docker): update dockerfile`          |
| `revert`   | Revert        | `revert: feat(auth): add user registration` |

### Scopes (Modules)

| Scope          | Description                  |
| -------------- | ---------------------------- |
| `auth`         | Authentication/Authorization |
| `identity`     | Identity module              |
| `forum`        | Forum module                 |
| `learning`     | Learning resources module    |
| `chat`         | Chat module                  |
| `career`       | Career hub module            |
| `notification` | Notification module          |
| `ai`           | AI module                    |
| `api`          | API layer                    |
| `shared`       | Shared kernel                |
| `infra`        | Infrastructure               |
| `frontend`     | Frontend app                 |
| `deps`         | Dependencies                 |
| `ci`           | CI/CD                        |
| `docker`       | Docker                       |

### Examples

```bash
# Good ✅
feat(identity): add user registration endpoint

Implement user registration with email verification.
- Add RegisterUserCommand and handler
- Add email validation
- Add password hashing

Refs: TASK-001

# Good ✅
fix(forum): resolve null reference in post query

The GetPostById query was throwing NullReferenceException
when the post had no comments.

Fixes: TASK-023

# Good ✅
refactor(shared): extract Result pattern to separate file

# Bad ❌
fixed stuff
update
WIP
feat: add feature   # Missing scope
```

---

## 🔄 WORKFLOW STEPS

### 1. Starting New Work

```bash
# 1. Ensure you're on develop and up to date
git checkout develop
git pull origin develop

# 2. Create feature branch
git checkout -b feature/TASK-001-user-authentication

# 3. Make changes and commit frequently
git add .
git commit -m "feat(identity): add user entity"

git add .
git commit -m "feat(identity): add user repository interface"

# ... more commits ...
```

### 2. Completing Work

```bash
# 1. Ensure branch is up to date with develop
git checkout develop
git pull origin develop
git checkout feature/TASK-001-user-authentication
git rebase develop

# 2. Push to remote
git push -u origin feature/TASK-001-user-authentication

# 3. Create Pull Request on GitHub
# - Title: feat(identity): implement user authentication [TASK-001]
# - Description: Use PR template
# - Request review (if team)

# 4. After PR approved and merged
git checkout develop
git pull origin develop
git branch -d feature/TASK-001-user-authentication
```

### 3. Hotfix Flow

```bash
# 1. Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix/TASK-099-critical-fix

# 2. Make fix
git add .
git commit -m "fix(auth): patch security vulnerability"

# 3. Merge to main AND develop
git checkout main
git merge hotfix/TASK-099-critical-fix
git push origin main

git checkout develop
git merge hotfix/TASK-099-critical-fix
git push origin develop

# 4. Delete hotfix branch
git branch -d hotfix/TASK-099-critical-fix
```

---

## 📋 PULL REQUEST TEMPLATE

Tạo file `.github/PULL_REQUEST_TEMPLATE.md`:

```markdown
## 📋 Description

<!-- Describe your changes in detail -->

## 🎫 Related Task

<!-- Link to task: TASK-XXX -->

## 🔄 Type of Change

- [ ] ✨ New feature
- [ ] 🐛 Bug fix
- [ ] 🔨 Refactoring
- [ ] 📝 Documentation
- [ ] 🧪 Tests
- [ ] 🔧 Configuration

## ✅ Checklist

- [ ] Code follows project conventions (RULES.md)
- [ ] Self-review completed
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No console.log/debug statements
- [ ] Task status updated in docs/tasks/

## 📸 Screenshots (if applicable)

<!-- Add screenshots here -->

## 🧪 How to Test

<!-- Steps to test your changes -->

1.
2.
3.
```

---

## 🏷️ VERSION TAGGING

### Semantic Versioning

```
v{MAJOR}.{MINOR}.{PATCH}

MAJOR: Breaking changes
MINOR: New features (backward compatible)
PATCH: Bug fixes (backward compatible)
```

### Examples

```bash
# After significant release
git tag -a v1.0.0 -m "Release v1.0.0 - MVP Launch"
git push origin v1.0.0

# After adding new feature
git tag -a v1.1.0 -m "Release v1.1.0 - Add chat module"
git push origin v1.1.0

# After bug fix
git tag -a v1.1.1 -m "Release v1.1.1 - Fix login issue"
git push origin v1.1.1
```

---

## 🚫 RULES

### DO ✅

- Commit frequently with meaningful messages
- Keep commits atomic (one logical change per commit)
- Rebase feature branches before merging
- Delete merged branches
- Update task status after completing work

### DON'T ❌

- Commit directly to `main` or `develop`
- Force push to shared branches
- Merge without PR (except hotfixes by lead)
- Leave uncommitted changes when switching branches
- Commit sensitive data (use .gitignore)

---

## 📁 .GITIGNORE ESSENTIALS

```gitignore
# .NET
bin/
obj/
*.user
*.userosscache
*.suo
.vs/

# Node/Frontend
node_modules/
.next/
out/
dist/

# Environment
.env
.env.local
.env.*.local
appsettings.Development.json
appsettings.Local.json

# IDE
.idea/
.vscode/
*.swp

# OS
.DS_Store
Thumbs.db

# Logs
logs/
*.log

# Build
publish/
```

---

_Last Updated: 2026-02-04_
