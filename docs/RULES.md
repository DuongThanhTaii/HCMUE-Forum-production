# 📐 CODING RULES & CONVENTIONS

> **Tất cả agents và developers PHẢI tuân thủ các rules này.**

---

## 📁 1. PROJECT STRUCTURE RULES

### 1.1 Solution Structure

```
Modules/
└── {ModuleName}/
    ├── UniHub.{ModuleName}.Domain/           # Domain layer (innermost)
    ├── UniHub.{ModuleName}.Application/      # Application layer
    ├── UniHub.{ModuleName}.Infrastructure/   # Infrastructure layer
    └── UniHub.{ModuleName}.Presentation/     # Presentation layer (API)
```

### 1.2 Layer Dependencies (STRICT)

```
Presentation → Application → Domain
                    ↓
              Infrastructure
```

- ❌ Domain KHÔNG được reference bất kỳ layer nào
- ❌ Application KHÔNG được reference Infrastructure hoặc Presentation
- ✅ Infrastructure implements interfaces từ Application
- ✅ Presentation chỉ reference Application

### 1.3 Pragmatic Implementation Rules (NEW)

Nguyên tắc này đảm bảo team **không over-engineer**, nhưng vẫn giữ chất lượng ở các luồng rủi ro cao.

- ✅ **Simple flows can be simple**: use-case đọc dữ liệu đơn giản, ít business logic có thể dùng cách triển khai tối giản.
- ✅ **Critical flows must be rigorous**: auth/permission/security/audit/payment phải theo đầy đủ pattern (validation, logging, test, audit).
- ✅ **Risk-based decision**: mức độ "bài bản" tỉ lệ thuận với impact nếu sai.
- ✅ **Consistency per use-case family**: cùng một nhóm use-case trong module phải theo pattern nhất quán.
- ✅ **Refactor trigger**: nếu flow tối giản bắt đầu có nhiều rule/side-effects, phải nâng cấp lên kiến trúc bài bản.

#### Decision Matrix (Pragmatic vs Rigorous)

| Use-case type | Recommended approach |
| --- | --- |
| Simple read/list, low risk | Pragmatic (tối giản) |
| Complex business workflow | Rigorous (CQRS đầy đủ) |
| Authorization / permission / endpoint toggle | Rigorous bắt buộc |
| Security-sensitive mutation | Rigorous bắt buộc |

#### Mandatory for Dynamic Authorization

- ✅ Deny-by-default
- ✅ Deny wins over allow
- ✅ Every permission mutation must have audit log
- ✅ Endpoint toggle must support emergency rollback

---

## 🏷️ 2. NAMING CONVENTIONS

### 2.1 C# Backend

| Type              | Convention  | Example                            |
| ----------------- | ----------- | ---------------------------------- |
| **Namespace**     | PascalCase  | `UniHub.Identity.Domain`           |
| **Class**         | PascalCase  | `UserService`, `PostAggregate`     |
| **Interface**     | IPascalCase | `IUserRepository`, `IEmailService` |
| **Method**        | PascalCase  | `GetUserById`, `CreatePost`        |
| **Property**      | PascalCase  | `FirstName`, `CreatedAt`           |
| **Private field** | \_camelCase | `_userRepository`, `_logger`       |
| **Parameter**     | camelCase   | `userId`, `postRequest`            |
| **Constant**      | UPPER_SNAKE | `MAX_RETRY_COUNT`                  |
| **Enum**          | PascalCase  | `UserStatus.Active`                |

### 2.2 TypeScript Frontend

| Type                 | Convention             | Example                      |
| -------------------- | ---------------------- | ---------------------------- |
| **Component**        | PascalCase             | `UserProfile.tsx`            |
| **Hook**             | camelCase + use prefix | `useAuth`, `usePosts`        |
| **Function**         | camelCase              | `fetchUsers`, `handleSubmit` |
| **Variable**         | camelCase              | `isLoading`, `userData`      |
| **Constant**         | UPPER_SNAKE            | `API_BASE_URL`               |
| **Type/Interface**   | PascalCase             | `User`, `PostResponse`       |
| **File (component)** | PascalCase             | `UserCard.tsx`               |
| **File (util)**      | kebab-case             | `api-client.ts`              |

### 2.3 Database

| Type            | Convention          | Example                      |
| --------------- | ------------------- | ---------------------------- |
| **Table**       | PascalCase (plural) | `Users`, `Posts`, `Comments` |
| **Column**      | PascalCase          | `FirstName`, `CreatedAt`     |
| **Primary Key** | Id                  | `Id`                         |
| **Foreign Key** | {Entity}Id          | `UserId`, `PostId`           |
| **Index**       | IX*{Table}*{Column} | `IX_Users_Email`             |

---

## 🏗️ 3. DDD PATTERNS

### 3.1 Entity Rules

```csharp
// ✅ CORRECT
public class User : Entity<UserId>
{
    public Email Email { get; private set; }  // Value Object
    public string PasswordHash { get; private set; }

    private User() { } // EF Core constructor

    public static User Create(Email email, string passwordHash)
    {
        var user = new User
        {
            Id = UserId.CreateUnique(),
            Email = email,
            PasswordHash = passwordHash
        };
        user.AddDomainEvent(new UserCreatedEvent(user.Id));
        return user;
    }

    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail;
        AddDomainEvent(new UserEmailChangedEvent(Id, newEmail));
    }
}

// ❌ WRONG - Public setters, no encapsulation
public class User
{
    public string Email { get; set; }
}
```

### 3.2 Value Object Rules

```csharp
// ✅ CORRECT
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(EmailErrors.Empty);

        if (!IsValidEmail(email))
            return Result.Failure<Email>(EmailErrors.InvalidFormat);

        return Result.Success(new Email(email.ToLowerInvariant()));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### 3.3 Aggregate Root Rules

```csharp
// ✅ CORRECT
public class Post : AggregateRoot<PostId>
{
    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public void AddComment(Comment comment)
    {
        _comments.Add(comment);
        AddDomainEvent(new CommentAddedEvent(Id, comment.Id));
    }
}

// ❌ WRONG - Exposing mutable collection
public class Post
{
    public List<Comment> Comments { get; set; }
}
```

### 3.4 Repository Rules

```csharp
// ✅ CORRECT - Repository chỉ cho Aggregate Root
public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}

// ❌ WRONG - Repository cho Entity không phải Aggregate Root
public interface ICommentRepository { } // Comment thuộc Post Aggregate
```

---

## 📨 4. CQRS PATTERNS

### 4.1 Command Rules

```csharp
// ✅ CORRECT
public sealed record CreatePostCommand(
    string Title,
    string Content,
    Guid AuthorId,
    Guid CategoryId
) : ICommand<Guid>;

public sealed class CreatePostCommandHandler
    : ICommandHandler<CreatePostCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreatePostCommand command,
        CancellationToken ct)
    {
        // Validation đã qua FluentValidation pipeline
        // Business logic here
    }
}
```

### 4.2 Query Rules

```csharp
// ✅ CORRECT
public sealed record GetPostByIdQuery(Guid PostId) : IQuery<PostResponse>;

public sealed class GetPostByIdQueryHandler
    : IQueryHandler<GetPostByIdQuery, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(
        GetPostByIdQuery query,
        CancellationToken ct)
    {
        // Read from optimized read model/view
    }
}
```

### 4.3 Validation Rules

```csharp
// ✅ CORRECT - FluentValidation
public sealed class CreatePostCommandValidator
    : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title max 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");
    }
}
```

---

## 🌐 5. API DESIGN RULES

### 5.1 Endpoint Naming

```
✅ CORRECT:
GET    /api/v1/posts
GET    /api/v1/posts/{id}
POST   /api/v1/posts
PUT    /api/v1/posts/{id}
DELETE /api/v1/posts/{id}
GET    /api/v1/posts/{id}/comments
POST   /api/v1/posts/{id}/comments

❌ WRONG:
GET    /api/v1/getPosts
POST   /api/v1/createPost
GET    /api/v1/post/getById
```

### 5.2 Response Format (MANDATORY)

Tất cả REST endpoint phải trả về envelope thống nhất `ApiResponse<T>`:

```json
{
    "success": true,
    "data": {},
    "message": null,
    "error": null
}
```

- `success`: bắt buộc, kiểu `boolean`
- `data`: payload business (hoặc `null` với command endpoint không cần body dữ liệu)
- `message`: thông điệp thành công dạng ngắn gọn (nullable)
- `error`: chuỗi lỗi chuẩn hóa cho FE (nullable)

✅ **Success Response**

```json
{
    "success": true,
    "data": {
        "id": "..."
    },
    "message": null,
    "error": null
}
```

✅ **Command-style Success (no data)**

```json
{
    "success": true,
    "data": null,
    "message": "Updated successfully",
    "error": null
}
```

✅ **Error Response**

```json
{
    "success": false,
    "data": null,
    "message": null,
    "error": "User with ID 'xxx' was not found"
}
```

✅ **Pagination Response** (`data` chứa paging object)

```json
{
    "success": true,
    "data": {
        "items": [],
        "pageNumber": 1,
        "pageSize": 10,
        "totalCount": 100,
        "totalPages": 10,
        "hasNextPage": true,
        "hasPreviousPage": false
    },
    "message": null,
    "error": null
}
```

❌ Không trả raw object/list trực tiếp ở top-level cho API mới.

### 5.3 HTTP Status Codes

| Code | Usage                          |
| ---- | ------------------------------ |
| 200  | Success (GET, PUT)             |
| 201  | Created (POST)                 |
| 204  | Legacy only (không dùng cho API mới nếu yêu cầu envelope) |
| 400  | Bad Request (Validation error) |
| 401  | Unauthorized                   |
| 403  | Forbidden                      |
| 404  | Not Found                      |
| 409  | Conflict                       |
| 500  | Internal Server Error          |

### 5.4 Migration Enforcement Rule (NEW)

- Từ thời điểm rule này có hiệu lực, mọi endpoint mới/refactor phải theo `ApiResponse<T>`.
- Endpoint cũ chưa migrate phải được đưa vào backlog refactor theo wave và không mở rộng contract raw-response thêm nữa.
- Khi update docs API, ví dụ response bắt buộc thể hiện envelope trước, payload business đặt trong `data`.

---

## 🎨 6. FRONTEND RULES

### 6.1 Component Structure

```tsx
// ✅ CORRECT - components/features/posts/PostCard.tsx
import { Card, CardContent, CardHeader } from "@/components/ui/card";

interface PostCardProps {
  post: Post;
  onLike?: (postId: string) => void;
}

export function PostCard({ post, onLike }: PostCardProps) {
  return (
    <Card>
      <CardHeader>{post.title}</CardHeader>
      <CardContent>{post.content}</CardContent>
    </Card>
  );
}
```

### 6.2 Shadcn/ui Usage

```tsx
// ✅ CORRECT - Use Shadcn components
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Dialog, DialogContent, DialogHeader } from "@/components/ui/dialog";

// ❌ WRONG - Don't create custom basic components
const MyButton = styled.button`...`; // Don't do this
```

### 6.3 State Management

```tsx
// ✅ Server State - TanStack Query
const { data, isLoading } = useQuery({
  queryKey: ["posts", postId],
  queryFn: () => fetchPost(postId),
});

// ✅ Client State - Zustand
const useAuthStore = create<AuthState>((set) => ({
  user: null,
  setUser: (user) => set({ user }),
}));
```

### 6.4 File Organization

```
components/
├── ui/                    # Shadcn components (auto-generated)
│   ├── button.tsx
│   ├── input.tsx
│   └── ...
├── features/              # Feature-specific components
│   ├── auth/
│   │   ├── LoginForm.tsx
│   │   └── RegisterForm.tsx
│   ├── posts/
│   │   ├── PostCard.tsx
│   │   ├── PostList.tsx
│   │   └── CreatePostForm.tsx
│   └── ...
└── shared/                # Shared components
    ├── Layout.tsx
    ├── Navbar.tsx
    └── Footer.tsx
```

---

## 🧪 7. TESTING RULES

### 7.1 Test Naming

```csharp
// ✅ CORRECT - MethodName_Scenario_ExpectedResult
[Fact]
public async Task CreatePost_WithValidData_ReturnsSuccess()

[Fact]
public async Task CreatePost_WithEmptyTitle_ReturnsValidationError()

// ❌ WRONG
[Fact]
public async Task Test1()
```

### 7.2 Test Structure (AAA)

```csharp
[Fact]
public async Task CreatePost_WithValidData_ReturnsSuccess()
{
    // Arrange
    var command = new CreatePostCommand("Title", "Content", userId, categoryId);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
}
```

### 7.3 Test Coverage Requirements

| Layer          | Minimum Coverage |
| -------------- | ---------------- |
| Domain         | 90%              |
| Application    | 80%              |
| Infrastructure | 70%              |
| Presentation   | 60%              |

---

## 📝 8. DOCUMENTATION RULES

### 8.1 Code Comments

```csharp
// ✅ CORRECT - Explain WHY, not WHAT
// Using eventual consistency here because immediate consistency
// would require distributed transaction across services
await _eventBus.PublishAsync(domainEvent);

// ❌ WRONG - Obvious comment
// Create a new user
var user = User.Create(email, password);
```

### 8.2 XML Documentation (Public APIs)

```csharp
/// <summary>
/// Creates a new post in the forum.
/// </summary>
/// <param name="command">The command containing post details.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The ID of the created post.</returns>
/// <exception cref="ValidationException">Thrown when validation fails.</exception>
public async Task<Result<Guid>> Handle(CreatePostCommand command, CancellationToken ct)
```

---

## 🔐 9. SECURITY RULES

### 9.1 Authentication

- ✅ Use JWT tokens with short expiry (15 mins)
- ✅ Implement refresh token rotation
- ✅ Store tokens in HttpOnly cookies (frontend)
- ❌ NEVER store tokens in localStorage

### 9.2 Authorization

```csharp
// ✅ CORRECT - Use policy-based authorization
[Authorize(Policy = "CanModeratePost")]
public async Task<IActionResult> DeletePost(Guid id)

// ❌ WRONG - Role checks in controller
if (User.IsInRole("Admin") || User.IsInRole("Moderator"))
```

### 9.3 Data Validation

- ✅ Validate ALL input at API boundary
- ✅ Use FluentValidation for complex rules
- ✅ Sanitize user content (XSS prevention)
- ❌ NEVER trust client-side validation alone

---

## 📦 10. DEPENDENCY RULES

### 10.1 Package Management

- Use Central Package Management (`Directory.Packages.props`)
- Pin exact versions, no floating versions
- Review security advisories before updating

### 10.2 Dependency Injection

```csharp
// ✅ CORRECT - Register in module's DI extension
public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();
        return services;
    }
}

// ❌ WRONG - Register in Program.cs directly
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

---

_Last Updated: 2026-02-04_
