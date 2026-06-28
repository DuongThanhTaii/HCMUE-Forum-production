# Phase 9.6: Backend Hardening & Production Readiness

## Overview

Tăng cường backend để sẵn sàng production: đăng ký MediatR pipeline behaviors, thay thế Swagger bằng Scalar UI, thêm rate limiting, tạo NotificationHub, dọn dẹp Program.cs và bảo mật credentials.

## Branch

`feature/phase-9.6-backend-hardening` → merged to `develop`

---

## TASK-108: MediatR Pipeline Behaviors + FluentValidation ✅

### Thay đổi:

- **ValidationBehavior.cs**: Throw `SharedKernel.Exceptions.ValidationException` thay vì `FluentValidation.ValidationException` — đảm bảo `GlobalExceptionHandler` bắt đúng exception
- **Program.cs**: Đăng ký 4 MediatR pipeline behaviors:
  - `ValidationBehavior<,>` — chạy FluentValidation trước mỗi command/query
  - `LoggingBehavior<,>` — log request/response
  - `PerformanceBehavior<,>` — cảnh báo query chậm
  - `UnhandledExceptionBehavior<,>` — log exception chưa xử lý
- **Program.cs**: Đăng ký FluentValidation validators từ 5 module assemblies (Identity, Forum, Learning, Chat, Career)
- **ForbiddenException.cs**: Tạo exception mới cho HTTP 403
- **GlobalExceptionHandler.cs**: Thêm mapping ForbiddenException → 403

### Files changed:

- `src/Shared/UniHub.Infrastructure/Behaviors/ValidationBehavior.cs`
- `src/Shared/UniHub.SharedKernel/Exceptions/ForbiddenException.cs` (NEW)
- `src/UniHub.API/Middlewares/GlobalExceptionHandler.cs`
- `src/UniHub.API/Program.cs`
- `src/UniHub.API/UniHub.API.csproj` (thêm `FluentValidation.DependencyInjectionExtensions`)

---

## TASK-109: Scalar API Docs + Program.cs Cleanup ✅

### Thay đổi:

- **Thay Swashbuckle bằng Scalar.AspNetCore**: Swashbuckle 7.0.0 không tương thích Microsoft.OpenApi 2.0.0 trong .NET 10. Scalar hiện đại hơn, tương thích tốt.
  - UI tại `/scalar/v1` với theme BluePlanet, hỗ trợ Bearer auth
- **CORS**: Đổi tên policy từ `"ChatCorsPolicy"` → `"DefaultCorsPolicy"`, origins configurable qua `Cors:AllowedOrigins`
- **Response Compression**: Thêm `AddResponseCompression` + `UseResponseCompression`
- **Dọn dẹp**:
  - Xóa WeatherForecast endpoint và record
  - Xóa duplicate `AddHealthChecks()` (đã có trong Infrastructure DI)
  - Fix health/connections kiểm tra `DefaultConnection` thay vì `PostgreSQL`
- **Config**:
  - Chat BaseUrl configurable qua `Chat:BaseUrl`
  - JWT SecretKey → placeholder (dùng User Secrets hoặc env variable)
  - Xóa real credentials khỏi `appsettings.Development.json`
  - Tạo `appsettings.Development.example.json` template
  - Untrack `appsettings.Development.json` khỏi git

### Files changed:

- `src/UniHub.API/Program.cs`
- `src/UniHub.API/UniHub.API.csproj`
- `Directory.Packages.props` (thêm Scalar.AspNetCore 2.0.36)
- `src/UniHub.API/appsettings.json`
- `src/UniHub.API/appsettings.Development.json`
- `src/UniHub.API/appsettings.Development.example.json` (NEW)

---

## TASK-110: Rate Limiting + NotificationHub ✅

### Rate Limiting:

- **Global**: 100 requests/phút per IP (FixedWindow)
- **auth**: 10 requests/phút — chống brute-force login
- **ai**: 20 requests/phút — bảo vệ AI endpoint tốn kém
- `[EnableRateLimiting("auth")]` trên `AuthController`
- `[EnableRateLimiting("ai")]` trên 4 AI controllers

### NotificationHub:

- SignalR hub tại `/hubs/notifications` với `[Authorize]`
- Group-based messaging: mỗi user join group `user-{userId}`
- Client methods: `ReceiveNotification`, `UnreadCountUpdated`
- Server methods: `MarkAsRead`, `MarkAllAsRead`
- Interface `INotificationClient` cho strongly-typed hub

### Files changed:

- `src/UniHub.API/Program.cs`
- `src/Modules/Notification/UniHub.Notification.Presentation/Hubs/NotificationHub.cs` (NEW)
- `src/Modules/Identity/UniHub.Identity.Presentation/Controllers/AuthController.cs`
- `src/Modules/AI/UniHub.AI.Presentation/Controllers/AIChatController.cs`
- `src/Modules/AI/UniHub.AI.Presentation/Controllers/ContentModerationController.cs`
- `src/Modules/AI/UniHub.AI.Presentation/Controllers/SmartSearchController.cs`
- `src/Modules/AI/UniHub.AI.Presentation/Controllers/SummarizationController.cs`

---

## Test Fixes ✅

### Bugs phát hiện và sửa:

1. **ValidationBehaviorTests**: Expected exception type thay đổi từ `FluentValidation.ValidationException` → `UniHub.SharedKernel.Exceptions.ValidationException`
2. **ApplicationDbContextTests**: TestDbContext kế thừa `ApplicationDbContext` → discover 20+ module entities → InMemory provider lỗi PK. **Fix**: TestDbContext kế thừa `DbContext` trực tiếp.
3. **RefreshTokenRepositoryTests**: Tương tự — dùng reflection `ModelBuilder.Ignore<T>()` để loại bỏ non-Identity entities.
4. **RefreshTokenRepository** (pre-existing bug): Dùng computed properties (`IsActive`, `IsExpired`) trong LINQ queries → EF Core không translate được. **Fix**: Inline conditions (`RevokedAt == null && now < ExpiresAt`).

### Files changed:

- `tests/Shared/UniHub.Infrastructure.Tests/Behaviors/ValidationBehaviorTests.cs`
- `tests/Shared/UniHub.Infrastructure.Tests/Persistence/ApplicationDbContextTests.cs`
- `tests/Modules/Identity/UniHub.Identity.Infrastructure.Tests/Persistence/RefreshTokenRepositoryTests.cs`
- `src/Modules/Identity/UniHub.Identity.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs`

---

## Build & Test Status

- **Build**: 0 errors, 4 warnings (giảm từ 26)
- **Tests**: 1,241 passed, 0 failed ✅
- **Branch**: Merged to `develop` ✅
