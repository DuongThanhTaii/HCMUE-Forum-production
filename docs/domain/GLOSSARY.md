# 📖 UBIQUITOUS LANGUAGE GLOSSARY

> **Ngôn ngữ chung (Ubiquitous Language) cho UniHub Project**
>
> Tài liệu này định nghĩa tất cả thuật ngữ domain được sử dụng xuyên suốt hệ thống.

---

## 🎯 PURPOSE

Ubiquitous Language là ngôn ngữ được chia sẻ giữa:

- Developers (Backend, Frontend)
- Domain Experts (Stakeholders, Users)
- Documentation (Code, Docs, Diagrams)

**Rules:**

- ✅ Sử dụng CHÍNH XÁC các thuật ngữ trong glossary này
- ✅ Thuật ngữ phải nhất quán trong code, database, API, UI
- ❌ KHÔNG sử dụng các từ đồng nghĩa (synonyms) khác
- ❌ KHÔNG tự ý thêm thuật ngữ mới mà chưa update glossary

---

## 🔐 IDENTITY CONTEXT

### User (Người dùng)

**English:** User  
**Vietnamese:** Người dùng  
**Definition:** Một cá nhân có tài khoản trong hệ thống UniHub.

**Attributes:**

- UserId (GUID)
- Email
- PasswordHash
- FirstName, LastName
- Role
- CreatedAt, UpdatedAt

**Types:**

- **Student** (Sinh viên)
- **Teacher** (Giảng viên)
- **Staff** (Nhân viên phòng ban)
- **Alumni** (Cựu sinh viên)
- **Guest** (Khách)

---

### Role (Vai trò)

**English:** Role  
**Vietnamese:** Vai trò  
**Definition:** Nhóm quyền hạn được gán cho User.

**Examples:**

- `Student` - Sinh viên
- `Teacher` - Giảng viên
- `Moderator` - Người kiểm duyệt
- `Admin` - Quản trị viên
- `Recruiter` - Nhà tuyển dụng

---

### Permission (Quyền hạn)

**English:** Permission  
**Vietnamese:** Quyền hạn  
**Definition:** Một hành động cụ thể mà User có thể thực hiện.

**Examples:**

- `Forum.Post.Create`
- `Forum.Post.Delete`
- `Learning.Document.Approve`
- `User.Role.Assign`

---

### Official Badge (Huy hiệu chính thức)

**English:** Official Badge  
**Vietnamese:** Huy hiệu chính thức  
**Definition:** Dấu xác nhận tài khoản thuộc đơn vị chính thức của trường (khoa, phòng ban).

**Examples:**

- "Khoa Công nghệ Thông tin"
- "Phòng Đào tạo"
- "Đoàn Thanh niên"

---

### Verified Account (Tài khoản xác minh)

**English:** Verified Account  
**Vietnamese:** Tài khoản xác minh  
**Definition:** Tài khoản đã xác thực danh tính qua email trường (@hcmue.edu.vn).

---

## 📝 FORUM CONTEXT

### Post (Bài đăng)

**English:** Post  
**Vietnamese:** Bài đăng  
**Definition:** Một bài viết chính trong diễn đàn, có thể là câu hỏi, thảo luận, hoặc chia sẻ.

**Attributes:**

- PostId (GUID)
- Title (Tiêu đề)
- Content (Nội dung)
- AuthorId (UserId)
- CategoryId
- Tags[]
- VoteCount (Số lượt vote)
- CommentCount
- CreatedAt, UpdatedAt

**Types:**

- **Question** (Câu hỏi)
- **Discussion** (Thảo luận)
- **Announcement** (Thông báo)
- **Confession** (Ẩn danh)

---

### Thread (Chủ đề)

**English:** Thread  
**Vietnamese:** Chủ đề, chuỗi thảo luận  
**Definition:** Một Post kèm tất cả Comments của nó tạo thành một Thread.

**Note:** Thread = Post + Comments (hierarchy)

---

### Comment (Bình luận)

**English:** Comment  
**Vietnamese:** Bình luận  
**Definition:** Phản hồi cho một Post hoặc một Comment khác (nested).

**Attributes:**

- CommentId (GUID)
- PostId
- ParentCommentId (nullable - for nested comments)
- Content
- AuthorId
- VoteCount
- CreatedAt, UpdatedAt

---

### Vote (Bình chọn)

**English:** Vote  
**Vietnamese:** Bình chọn, Vote  
**Definition:** Hành động upvote (+1) hoặc downvote (-1) cho Post hoặc Comment.

**Attributes:**

- VoteId (GUID)
- TargetId (PostId or CommentId)
- TargetType (Post or Comment)
- UserId
- VoteType (Up or Down)
- CreatedAt

**Business Rules:**

- Một User chỉ vote 1 lần cho 1 target
- User có thể thay đổi vote (Up → Down hoặc ngược lại)
- User có thể remove vote

---

### Category (Danh mục)

**English:** Category  
**Vietnamese:** Danh mục  
**Definition:** Nhóm phân loại các Posts theo chủ đề.

**Examples:**

- "Học tập" (Academic)
- "Sinh hoạt" (Campus Life)
- "Nghề nghiệp" (Career)
- "Công nghệ" (Technology)
- "Ẩn danh" (Confessions)

---

### Tag (Thẻ)

**English:** Tag  
**Vietnamese:** Thẻ, nhãn  
**Definition:** Từ khóa gắn vào Post để phân loại chi tiết hơn Category.

**Examples:**

- `#lập-trình`
- `#thi-cuối-kỳ`
- `#học-bổng`
- `#thực-tập`

**Note:** Tags có thể tự do tạo bởi users (folksonomy)

---

### Confession (Bài ẩn danh)

**English:** Confession  
**Vietnamese:** Bài ẩn danh  
**Definition:** Một loại Post đặc biệt mà tác giả được ẩn danh.

**Business Rules:**

- AuthorId được mã hóa/ẩn trong UI
- Chỉ Admin/Moderator thấy tác giả thật
- Có thể bị xóa nếu vi phạm quy định

---

## 📚 LEARNING CONTEXT

### Document (Tài liệu)

**English:** Document  
**Vietnamese:** Tài liệu học tập  
**Definition:** File tài liệu học tập (PDF, DOCX, slides) được upload lên hệ thống.

**Attributes:**

- DocumentId (GUID)
- Title
- Description
- CourseId
- UploaderId (UserId)
- FileUrl
- FileSize
- ApprovalStatus (Pending, Approved, Rejected)
- RatingAverage
- DownloadCount
- CreatedAt, UpdatedAt

**Types:**

- **Lecture Notes** (Giáo trình)
- **Exam Papers** (Đề thi)
- **Assignments** (Bài tập)
- **Slides** (Bài giảng)

---

### Course (Học phần)

**English:** Course  
**Vietnamese:** Học phần, môn học  
**Definition:** Một môn học trong chương trình đào tạo.

**Attributes:**

- CourseId (GUID)
- Code (e.g., "CS101")
- Name (e.g., "Lập trình căn bản")
- FacultyId
- Credits
- Semester

**Examples:**

- CS101 - Lập trình căn bản
- MATH201 - Giải tích 2
- ENG301 - Tiếng Anh chuyên ngành

---

### Faculty (Khoa)

**English:** Faculty  
**Vietnamese:** Khoa  
**Definition:** Đơn vị đào tạo trong trường (ví dụ: Khoa Công nghệ Thông tin).

**Examples:**

- Khoa Công nghệ Thông tin
- Khoa Toán - Tin học
- Khoa Sư phạm
- Khoa Ngoại ngữ

---

### Approval (Phê duyệt)

**English:** Approval  
**Vietnamese:** Phê duyệt  
**Definition:** Quy trình kiểm duyệt tài liệu trước khi công khai.

**Statuses:**

- **Pending** - Đang chờ duyệt
- **Approved** - Đã duyệt
- **Rejected** - Từ chối

**Actors:**

- **Uploader** - Người upload
- **Moderator** - Người kiểm duyệt

---

### Moderator (Người kiểm duyệt)

**English:** Moderator  
**Vietnamese:** Người kiểm duyệt  
**Definition:** User có quyền duyệt/từ chối Documents hoặc Posts.

**Responsibilities:**

- Approve/Reject documents
- Delete inappropriate posts
- Ban users

---

### Semester (Học kỳ)

**English:** Semester  
**Vietnamese:** Học kỳ  
**Definition:** Kỳ học trong năm học (HK1, HK2, HK3).

**Examples:**

- HK1 2025-2026
- HK2 2025-2026
- HK Hè 2026

---

## 💬 CHAT CONTEXT

### Conversation (Cuộc trò chuyện)

**English:** Conversation  
**Vietnamese:** Cuộc trò chuyện  
**Definition:** Một thread chat giữa 2 hoặc nhiều Users.

**Types:**

- **Direct Message (DM)** - 1-to-1 chat
- **Group Chat** - nhiều users
- **Channel** - public chat room

---

### Message (Tin nhắn)

**English:** Message  
**Vietnamese:** Tin nhắn  
**Definition:** Một tin nhắn trong Conversation.

**Attributes:**

- MessageId (GUID)
- ConversationId
- SenderId (UserId)
- Content
- Attachments[]
- ReadBy[] (for group chats)
- CreatedAt, UpdatedAt, DeletedAt

**Types:**

- **Text** - Tin nhắn text
- **Image** - Hình ảnh
- **File** - File đính kèm
- **System** - Tin nhắn hệ thống (e.g., "User joined")

---

### Channel (Kênh)

**English:** Channel  
**Vietnamese:** Kênh  
**Definition:** Phòng chat công khai mà bất kỳ User nào cũng có thể tham gia.

**Examples:**

- #general
- #học-tập
- #tuyển-dụng
- #sự-kiện

---

### Group (Nhóm)

**English:** Group  
**Vietnamese:** Nhóm  
**Definition:** Cuộc trò chuyện giữa 3+ users, có thể private.

**Attributes:**

- GroupId (GUID)
- Name
- AvatarUrl
- Members[] (UserIds)
- AdminIds[]
- CreatedBy
- CreatedAt

---

### Direct Message (Tin nhắn riêng)

**English:** Direct Message (DM)  
**Vietnamese:** Tin nhắn riêng  
**Definition:** Cuộc trò chuyện 1-to-1 giữa 2 users.

**Note:** Là một loại Conversation đặc biệt với chỉ 2 participants.

---

## 💼 CAREER CONTEXT

### Job Posting (Tin tuyển dụng)

**English:** Job Posting  
**Vietnamese:** Tin tuyển dụng  
**Definition:** Thông tin về một vị trí tuyển dụng từ Company.

**Attributes:**

- JobPostingId (GUID)
- CompanyId
- Title (e.g., "Thực tập sinh Frontend")
- Description
- Requirements[]
- Benefits[]
- Location
- Salary (nullable)
- ExpiryDate
- PostedAt

---

### Company (Công ty)

**English:** Company  
**Vietnamese:** Công ty  
**Definition:** Nhà tuyển dụng, tổ chức đăng tin tuyển dụng.

**Attributes:**

- CompanyId (GUID)
- Name
- Description
- LogoUrl
- Website
- Industry (Ngành nghề)
- Size (Quy mô)
- VerifiedStatus (Đã xác thực hay chưa)

---

### Application (Đơn ứng tuyển)

**English:** Application  
**Vietnamese:** Đơn ứng tuyển  
**Definition:** Hồ sơ ứng tuyển của User cho một JobPosting.

**Attributes:**

- ApplicationId (GUID)
- JobPostingId
- UserId
- ResumeUrl
- CoverLetter
- Status (Pending, Reviewing, Accepted, Rejected)
- AppliedAt

---

### Recruiter (Nhà tuyển dụng)

**English:** Recruiter  
**Vietnamese:** Nhà tuyển dụng  
**Definition:** User đại diện cho Company, có quyền đăng tin tuyển dụng.

**Note:** Là một Role đặc biệt, liên kết với Company.

---

### Resume / CV (Hồ sơ)

**English:** Resume / CV  
**Vietnamese:** Hồ sơ xin việc, CV  
**Definition:** File PDF/DOCX chứa thông tin cá nhân, học vấn, kinh nghiệm của User.

**Note:** User có thể upload nhiều versions của CV.

---

## 🔔 NOTIFICATION CONTEXT

### Notification (Thông báo)

**English:** Notification  
**Vietnamese:** Thông báo  
**Definition:** Tin nhắn hệ thống gửi đến User về một sự kiện.

**Attributes:**

- NotificationId (GUID)
- UserId
- Type (e.g., PostComment, MessageReceived, JobPosted)
- Title
- Content
- Link (URL to related resource)
- ReadStatus (Read, Unread)
- CreatedAt

**Types:**

- **Post Comment** - Có người comment bài của bạn
- **Message Received** - Có tin nhắn mới
- **Job Posted** - Có việc làm mới phù hợp
- **Document Approved** - Tài liệu của bạn được duyệt

---

### Subscription (Đăng ký nhận)

**English:** Subscription  
**Vietnamese:** Đăng ký nhận thông báo  
**Definition:** User đăng ký nhận notifications cho một loại sự kiện.

**Examples:**

- Subscribe to Post → nhận thông báo khi có comment mới
- Subscribe to Category → nhận thông báo khi có post mới
- Subscribe to Job Tag → nhận thông báo việc làm phù hợp

---

### Digest (Tổng hợp)

**English:** Digest  
**Vietnamese:** Bản tin tổng hợp  
**Definition:** Email/notification tổng hợp các hoạt động trong một khoảng thời gian.

**Types:**

- **Daily Digest** - Tổng hợp hàng ngày
- **Weekly Digest** - Tổng hợp hàng tuần

---

## 🤖 AI CONTEXT

### AI Assistant (Trợ lý AI)

**English:** AI Assistant  
**Vietnamese:** Trợ lý AI  
**Definition:** Chatbot AI hỗ trợ users trả lời câu hỏi, tìm tài liệu.

**Capabilities:**

- Answer questions about courses
- Search documents
- Summarize long posts
- Suggest related content

---

### AI Chat Session (Phiên chat AI)

**English:** AI Chat Session  
**Vietnamese:** Phiên trò chuyện AI  
**Definition:** Một cuộc trò chuyện giữa User và AI Assistant.

**Attributes:**

- SessionId (GUID)
- UserId
- Messages[] (User messages + AI responses)
- Context (optional - for contextual chat)
- CreatedAt

---

### Prompt (Câu hỏi/Lệnh)

**English:** Prompt  
**Vietnamese:** Câu hỏi, lệnh  
**Definition:** Câu hỏi hoặc yêu cầu mà User gửi cho AI.

**Examples:**

- "Tìm tài liệu về Giải tích 2"
- "Tóm tắt bài viết này"
- "Gợi ý việc làm cho sinh viên IT"

---

### AI Response (Phản hồi AI)

**English:** AI Response  
**Vietnamese:** Câu trả lời AI  
**Definition:** Câu trả lời được tạo bởi AI Assistant cho User prompt.

---

## 🔄 SHARED CONCEPTS (Cross-Context)

### Aggregate (Tập hợp)

**English:** Aggregate  
**Vietnamese:** Tập hợp (DDD pattern)  
**Definition:** Một nhóm entities/value objects được coi như một đơn vị thống nhất về transaction.

**Example:** Post Aggregate = Post (root) + Comments + Votes

---

### Domain Event (Sự kiện domain)

**English:** Domain Event  
**Vietnamese:** Sự kiện nghiệp vụ  
**Definition:** Một sự kiện quan trọng xảy ra trong domain.

**Examples:**

- `UserRegisteredEvent`
- `PostCreatedEvent`
- `DocumentApprovedEvent`
- `MessageSentEvent`

---

### Entity (Thực thể)

**English:** Entity  
**Vietnamese:** Thực thể  
**Definition:** Object có identity (ID) và lifecycle.

**Examples:** User, Post, Document

---

### Value Object (Đối tượng giá trị)

**English:** Value Object  
**Vietnamese:** Đối tượng giá trị  
**Definition:** Object không có identity, chỉ xác định bởi các attributes.

**Examples:** Email, Address, Money, DateRange

---

### Repository (Kho)

**English:** Repository  
**Vietnamese:** Kho, Repository  
**Definition:** Interface để truy cập/lưu trữ Aggregates.

**Examples:**

- `IUserRepository`
- `IPostRepository`
- `IDocumentRepository`

---

## 📊 CROSS-CUTTING CONCERNS

### Audit (Kiểm toán)

**English:** Audit  
**Vietnamese:** Kiểm toán  
**Definition:** Ghi lại lịch sử thay đổi của entities.

**Attributes:**

- CreatedAt, CreatedBy
- UpdatedAt, UpdatedBy
- DeletedAt, DeletedBy (for soft delete)

---

### Pagination (Phân trang)

**English:** Pagination  
**Vietnamese:** Phân trang  
**Definition:** Chia kết quả thành nhiều trang.

**Attributes:**

- PageNumber (số trang)
- PageSize (số items/trang)
- TotalCount (tổng số items)
- TotalPages (tổng số trang)

---

### Filter (Lọc)

**English:** Filter  
**Vietnamese:** Bộ lọc  
**Definition:** Điều kiện để lọc kết quả query.

**Examples:**

- Filter posts by category
- Filter documents by course
- Filter jobs by location

---

### Sort (Sắp xếp)

**English:** Sort  
**Vietnamese:** Sắp xếp  
**Definition:** Thứ tự sắp xếp kết quả.

**Options:**

- CreatedAt DESC (mới nhất)
- VoteCount DESC (phổ biến nhất)
- Rating DESC (đánh giá cao nhất)

---

## 🔐 SECURITY TERMS

### JWT Token

**English:** JWT Token  
**Vietnamese:** JWT Token  
**Definition:** JSON Web Token dùng để xác thực User.

**Contains:**

- UserId
- Email
- Roles[]
- Expiration time

---

### Refresh Token

**English:** Refresh Token  
**Vietnamese:** Refresh Token  
**Definition:** Token dùng để lấy JWT Token mới khi hết hạn.

---

### Authorization (Phân quyền)

**English:** Authorization  
**Vietnamese:** Phân quyền  
**Definition:** Kiểm tra User có quyền thực hiện hành động hay không.

**Note:** Khác với Authentication (xác thực danh tính)

---

## 📝 NOTES FOR DEVELOPERS

### Naming in Code

```csharp
// ✅ CORRECT - Use exact glossary terms
public class Post { }
public class Comment { }
public interface IPostRepository { }

// ❌ WRONG - Don't use synonyms
public class Article { }  // Should be Post
public class Reply { }    // Should be Comment
```

### Naming in Database

```sql
-- ✅ CORRECT
CREATE TABLE Posts (...);
CREATE TABLE Comments (...);

-- ❌ WRONG
CREATE TABLE Articles (...);
```

### Naming in API

```
GET /api/posts          ✅
GET /api/articles       ❌

GET /api/documents      ✅
GET /api/files          ❌
```

### Naming in UI

```tsx
// ✅ CORRECT
<PostCard />
<CommentList />

// ❌ WRONG
<ArticleCard />
<ReplyList />
```

---

## 🔄 VERSIONING

| Version | Date       | Changes              | Author |
| ------- | ---------- | -------------------- | ------ |
| 1.0     | 2026-02-04 | Initial glossary     | Agent  |

---

## 📚 REFERENCES

- [Domain-Driven Design (Eric Evans)](https://www.domainlanguage.com/)
- [Implementing Domain-Driven Design (Vaughn Vernon)](https://vaughnvernon.com/)
- [AGENT_CONTEXT.md](../AGENT_CONTEXT.md)
- [BOUNDED_CONTEXTS.md](./BOUNDED_CONTEXTS.md)

---

_Last Updated: 2026-02-04_
