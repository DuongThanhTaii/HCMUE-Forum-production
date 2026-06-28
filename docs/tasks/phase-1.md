# 🎯 PHASE 1: STRATEGIC DDD - DOMAIN DISCOVERY

> **Phân tích domain, xác định Bounded Contexts, và vẽ Context Map**

---

## 📋 PHASE INFO

| Property          | Value            |
| ----------------- | ---------------- |
| **Phase**         | 1                |
| **Name**          | Domain Discovery |
| **Status**        | ✅ DONE          |
| **Progress**      | 5/5 tasks (100%) |
| **Est. Duration** | 1 week           |
| **Dependencies**  | Phase 0          |

---

## 🎯 OBJECTIVES

- [x] Xây dựng Ubiquitous Language (Glossary)
- [x] Xác định các Bounded Contexts
- [x] Vẽ Context Map với relationships
- [x] Phân loại Core/Supporting/Generic domains
- [x] Define module boundaries và integration contracts

---

## 📝 TASKS

### TASK-009: Create Ubiquitous Language Glossary

| Property         | Value                               |
| ---------------- | ----------------------------------- |
| **ID**           | TASK-009                            |
| **Status**       | ✅ COMPLETED                        |
| **Priority**     | 🔴 Critical                         |
| **Estimate**     | 3 hours                             |
| **Branch**       | `docs/TASK-009-ubiquitous-language` |
| **Dependencies** | Phase 0 completed                   |

**Description:**
Tạo glossary định nghĩa tất cả các thuật ngữ domain sử dụng trong hệ thống.

**Acceptance Criteria:**

- [x] Glossary document created
- [x] All core domain terms defined
- [x] Vietnamese and English terms mapped
- [x] Terms grouped by bounded context
- [x] Team reviewed and agreed

**Deliverable:**
Create `docs/domain/GLOSSARY.md`

**Terms to Define:**

```markdown
# Identity Context

- User (Người dùng)
- Role (Vai trò)
- Permission (Quyền hạn)
- Official Badge (Huy hiệu chính thức)
- Verified Account (Tài khoản xác minh)

# Forum Context

- Post (Bài đăng)
- Thread (Chủ đề)
- Comment (Bình luận)
- Vote (Bình chọn)
- Category (Danh mục)
- Tag (Thẻ)
- Confession (Bài ẩn danh)

# Learning Context

- Document (Tài liệu)
- Course (Học phần)
- Faculty (Khoa)
- Approval (Phê duyệt)
- Moderator (Người kiểm duyệt)
- Semester (Học kỳ)

# Chat Context

- Conversation (Cuộc trò chuyện)
- Message (Tin nhắn)
- Channel (Kênh)
- Group (Nhóm)
- Direct Message (Tin nhắn riêng)

# Career Context

- Job Posting (Tin tuyển dụng)
- Company (Công ty)
- Application (Đơn ứng tuyển)
- Recruiter (Nhà tuyển dụng)
- Resume/CV (Hồ sơ)

# Notification Context

- Notification (Thông báo)
- Subscription (Đăng ký nhận)
- Digest (Tổng hợp)
```

**Commit Message:**

```
docs(domain): create ubiquitous language glossary

- Define core domain terms
- Map Vietnamese and English terms
- Group terms by bounded context

Refs: TASK-009
```

---

### TASK-011: Create Context Map

| Property         | Value                       |
| ---------------- | --------------------------- |
| **ID**           | TASK-011                    |
| **Status**       | ✅ COMPLETED                |
| **Priority**     | 🔴 Critical                 |
| **Estimate**     | 3 hours                     |
| **Branch**       | `docs/TASK-011-context-map` |
| **Dependencies** | TASK-010                    |

**Description:**
Tạo context map thể hiện relationships giữa các bounded contexts.

**Acceptance Criteria:**

- [x] Context Map diagram created
- [x] Relationships clearly labeled
- [x] Integration patterns defined
- [x] Upstream/Downstream identified

**Deliverable:**
Create `docs/domain/CONTEXT_MAP.md`

**Context Relationships:**

1. Identity Context → All (Open Host Service)
2. All Contexts → Notification (Event-driven)
3. Core Contexts → AI (Anti-Corruption Layer)
4. AI → External APIs (Conformist)

**Commit Message:**

```
docs(domain): create context map with integration patterns

- Create comprehensive context map showing relationships
- Define 7 bounded contexts with integration patterns
- Document OHS, ACL, Conformist, Customer/Supplier patterns
- Add visual Mermaid diagram for context relationships
- Define communication mechanisms (sync/async)
- Document evolution strategy (monolith → microservices)

TASK-011
```

Refs: TASK-010

---

### TASK-010: Identify Bounded Contexts

| Property         | Value                            |
| ---------------- | -------------------------------- |
| **ID**           | TASK-010                         |
| **Status**       | ✅ COMPLETED                     |
| **Priority**     | 🔴 Critical                      |
| **Estimate**     | 4 hours                          |
| **Branch**       | `docs/TASK-010-bounded-contexts` |
| **Dependencies** | TASK-009                         |

**Description:**
Xác định và document tất cả bounded contexts trong hệ thống.

**Acceptance Criteria:**

- [x] All bounded contexts identified
- [x] Context responsibilities defined
- [x] Context boundaries clear
- [x] Aggregates per context listed
- [x] Diagram created

**Deliverable:**
Create `docs/domain/BOUNDED_CONTEXTS.md`

**Bounded Contexts:**

```markdown
## 1. Identity Context

Responsibilities:

- User registration and authentication
- Role and permission management
- Official badge/verification system
- Session management

Aggregates:

- User (root)
- Role (root)
- Permission (entity)

## 2. Forum Context

Responsibilities:

- Post creation and management
- Comments and discussions
- Voting system
- Category management
- Content moderation

Aggregates:

- Post (root)
- Category (root)
- Tag (entity)

## 3. Learning Context

Responsibilities:

- Document upload and storage
- Course/Subject management
- Approval workflow
- Faculty management
- Rating and reviews

Aggregates:

- Document (root)
- Course (root)
- Faculty (root)

## 4. Chat Context

Responsibilities:

- Real-time messaging
- Group conversations
- Channel management
- File sharing in chat
- Online presence

Aggregates:

- Conversation (root)
- Channel (root)
- Message (entity)

## 5. Career Context

Responsibilities:

- Job posting management
- Company profiles
- Application tracking
- Job matching

Aggregates:

- JobPosting (root)
- Company (root)
- Application (entity)

## 6. Notification Context

Responsibilities:

- Push notifications
- Email notifications
- In-app notifications
- Subscription management

Aggregates:

- Notification (root)
- Template (entity)
- Subscription (entity)

## 7. AI Context

Responsibilities:

- AI provider management
- Chatbot conversations
- Content moderation
- Smart features

Aggregates:

- AIConversation (root)
- AIProvider (entity)
```

**Commit Message:**

```
docs(domain): define bounded contexts

- Identify 7 bounded contexts
- Define responsibilities per context
- List aggregates per context
- Create context diagram

Refs: TASK-010
```

---

### TASK-012: Classify Domains

| Property         | Value                                 |
| ---------------- | ------------------------------------- |
| **ID**           | TASK-012                              |
| **Status**       | ✅ COMPLETED                          |
| **Priority**     | 🟡 Medium                             |
| **Estimate**     | 2 hours                               |
| **Branch**       | `docs/TASK-012-domain-classification` |
| **Dependencies** | TASK-010                              |

**Description:**
Phân loại các domains thành Core, Supporting, Generic.

**Acceptance Criteria:**

- [x] All domains classified
- [x] Classification rationale documented
- [x] Development priority determined
- [x] Resource allocation suggested

**Deliverable:**
Update `docs/domain/BOUNDED_CONTEXTS.md`

**Classification:**

```markdown
## Domain Classification

### Core Domains (Competitive Advantage)

Priority: Highest - Build in-house

| Domain   | Reason                                   |
| -------- | ---------------------------------------- |
| Identity | Custom RBAC with dynamic permissions     |
| Forum    | Main feature, UX differentiation         |
| Learning | Unique approval workflow, Event Sourcing |
| Chat     | Real-time UX is critical                 |

### Supporting Domains

Priority: Medium - Build with less complexity

| Domain       | Reason                   |
| ------------ | ------------------------ |
| Career       | Important but not unique |
| Notification | Standard patterns        |

### Generic Domains

Priority: Lower - Use existing solutions where possible

| Domain | Reason                           |
| ------ | -------------------------------- |
| AI     | Use external APIs (Groq, Gemini) |
```

**Commit Message:**

```
docs(domain): classify domains by strategic importance

- Classify core domains: Identity, Forum, Learning, Chat
- Classify supporting domains: Career, Notification
- Classify generic domains: AI
- Document rationale and priorities

Refs: TASK-012
```

---

### TASK-013: Define Module Integration Contracts

| Property         | Value                                 |
| ---------------- | ------------------------------------- |
| **ID**           | TASK-013                              |
| **Status**       | ✅ COMPLETED                          |
| **Priority**     | 🔴 Critical                           |
| **Estimate**     | 4 hours                               |
| **Branch**       | `docs/TASK-013-integration-contracts` |
| **Dependencies** | TASK-011                              |

**Description:**
Define contracts (DTOs, Events) cho communication giữa modules.

**Acceptance Criteria:**

- [x] Shared DTOs defined
- [x] Domain Events defined
- [x] Integration Events defined
- [x] API contracts documented

**Deliverable:**
Create `docs/domain/INTEGRATION_CONTRACTS.md`

**Contracts:**

```markdown
## Shared DTOs (UniHub.Contracts)

### Identity DTOs

- UserDto { Id, Email, DisplayName, AvatarUrl, Badge? }
- UserSummaryDto { Id, DisplayName, AvatarUrl }
- PermissionDto { Code, Name, Module }

### Common DTOs

- PagedResult<T> { Items, PageNumber, PageSize, TotalCount }
- Result<T> { IsSuccess, Value, Error }

## Domain Events (In-Process)

### Identity Events

- UserRegisteredEvent { UserId, Email, RegisteredAt }
- UserProfileUpdatedEvent { UserId }
- RoleAssignedEvent { UserId, RoleId }

### Forum Events

- PostCreatedEvent { PostId, AuthorId, CategoryId }
- PostPublishedEvent { PostId }
- CommentAddedEvent { PostId, CommentId, AuthorId }
- PostVotedEvent { PostId, UserId, VoteType }

### Learning Events

- DocumentSubmittedEvent { DocumentId, UploaderId }
- DocumentApprovedEvent { DocumentId, ApproverId }
- DocumentRejectedEvent { DocumentId, Reason }

### Chat Events

- MessageSentEvent { ConversationId, SenderId, MessageId }
- ConversationCreatedEvent { ConversationId, ParticipantIds }

### Career Events

- JobPostedEvent { JobId, CompanyId }
- ApplicationSubmittedEvent { ApplicationId, JobId, ApplicantId }

## Integration Events (Cross-Module)

### For Notification Module

- NotifyUserEvent { UserId, Title, Body, Type, Data }
- NotifyGroupEvent { UserIds, Title, Body, Type, Data }
- BroadcastEvent { Title, Body, Type, Data }
```

**Commit Message:**

```
docs(domain): define module integration contracts

- Define shared DTOs
- Define domain events per context
- Define integration events
- Document event payloads

Refs: TASK-013
```

---

## ✅ COMPLETION CHECKLIST

- [x] TASK-009: Create Ubiquitous Language Glossary
- [x] TASK-010: Identify Bounded Contexts
- [x] TASK-011: Create Context Map
- [x] TASK-012: Classify Domains
- [x] TASK-013: Define Integration Contracts

---

## 📝 NOTES

- Phase này chủ yếu là documentation
- Tất cả documents phải được review trước khi implement code
- Glossary sẽ được update liên tục trong quá trình phát triển

---

_Last Updated: 2026-02-04_
