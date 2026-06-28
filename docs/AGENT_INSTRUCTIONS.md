# 🤖 AGENT INSTRUCTIONS

> **Hướng dẫn chi tiết cho AI Agent (Claude Sonnet 4.5) khi implement tasks**

---

## 📋 TRƯỚC KHI BẮT ĐẦU

### 1. Đọc các file theo thứ tự

```
1. docs/AGENT_CONTEXT.md      → Hiểu tổng quan project
2. docs/RULES.md              → Nắm coding conventions
3. docs/ARCHITECTURE.md       → Hiểu kiến trúc hệ thống
4. docs/GIT_WORKFLOW.md       → Hiểu quy trình Git
5. docs/tasks/STATUS.md       → Xem progress hiện tại
6. docs/tasks/phase-X.md      → Đọc task cần implement
```

### 2. Xác định Task cần làm

- Tìm task đầu tiên có status `⬜ NOT_STARTED`
- Kiểm tra `Dependencies` - các task phụ thuộc đã `✅ COMPLETED` chưa
- Nếu dependencies chưa done → Không được bắt đầu task này

---

## 🚀 QUY TRÌNH IMPLEMENT TASK

### Step 1: Tạo Branch

```bash
# Format: feature/TASK-XXX-description
git checkout develop
git pull origin develop
git checkout -b feature/TASK-001-solution-structure
```

### Step 2: Implement theo Acceptance Criteria

- Đọc kỹ `Acceptance Criteria` trong task
- Implement từng item một, đánh dấu ✅ khi xong
- Tham khảo `Expected Output` để biết kết quả mong đợi

### Step 3: Verify Implementation

```bash
# Backend
dotnet build
dotnet test

# Frontend
npm run lint
npm run build
npm run test
```

### Step 4: Commit với Message đã định sẵn

```bash
# Copy chính xác commit message từ task file
git add .
git commit -m "feat(infra): create solution structure for DDD architecture

- Add UniHub.sln solution file
- Create folder structure for modules
- Add Directory.Build.props for common settings
- Add Directory.Packages.props for central package management

Refs: TASK-001"
```

### Step 5: Push và Tạo PR

```bash
git push -u origin feature/TASK-001-solution-structure
```

### Step 6: Update Task Status

Sau khi merge PR, update file `docs/tasks/phase-X.md`:

```markdown
| **Status** | ✅ COMPLETED |
```

---

## 📝 CODE TEMPLATES

### Template 1: Entity (Domain Layer)

```csharp
// File: src/Modules/{Module}/UniHub.{Module}.Domain/Entities/{Entity}.cs
using UniHub.SharedKernel.Domain;

namespace UniHub.{Module}.Domain.Entities;

public sealed class {Entity} : Entity<{Entity}Id>, IAggregateRoot
{
    // Properties (private set)
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private {Entity}() { }

    // Factory method
    public static {Entity} Create(string name)
    {
        var entity = new {Entity}
        {
            Id = {Entity}Id.CreateUnique(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        entity.AddDomainEvent(new {Entity}CreatedDomainEvent(entity.Id));
        return entity;
    }

    // Domain methods
    public void UpdateName(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new {Entity}UpdatedDomainEvent(Id));
    }
}
```

### Template 2: Value Object (Domain Layer)

```csharp
// File: src/Modules/{Module}/UniHub.{Module}.Domain/ValueObjects/{ValueObject}.cs
using UniHub.SharedKernel.Domain;

namespace UniHub.{Module}.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty");

        if (!email.Contains('@'))
            throw new DomainException("Invalid email format");

        return new Email(email.ToLower().Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Template 3: Command (Application Layer)

```csharp
// File: src/Modules/{Module}/UniHub.{Module}.Application/Commands/{Command}/{Command}Command.cs
using MediatR;
using UniHub.SharedKernel.Application;

namespace UniHub.{Module}.Application.Commands.{Command};

public sealed record {Command}Command(
    string Property1,
    int Property2
) : ICommand<{Command}Response>;

public sealed record {Command}Response(Guid Id);
```

### Template 4: Command Handler (Application Layer)

```csharp
// File: src/Modules/{Module}/UniHub.{Module}.Application/Commands/{Command}/{Command}CommandHandler.cs
using MediatR;
using UniHub.SharedKernel.Application;

namespace UniHub.{Module}.Application.Commands.{Command};

internal sealed class {Command}CommandHandler
    : ICommandHandler<{Command}Command, {Command}Response>
{
    private readonly I{Entity}Repository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public {Command}CommandHandler(
        I{Entity}Repository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<{Command}Response>> Handle(
        {Command}Command request,
        CancellationToken cancellationToken)
    {
        // 1. Create domain entity
        var entity = {Entity}.Create(request.Property1);

        // 2. Persist
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Return response
        return new {Command}Response(entity.Id.Value);
    }
}
```

### Template 5: Query (Application Layer)

```csharp
// File: src/Modules/{Module}/UniHub.{Module}.Application/Queries/{Query}/{Query}Query.cs
using MediatR;
using UniHub.SharedKernel.Application;

namespace UniHub.{Module}.Application.Queries.{Query};

public sealed record {Query}Query(Guid Id) : IQuery<{Query}Response>;

public sealed record {Query}Response(
    Guid Id,
    string Name,
    DateTime CreatedAt
);
```

### Template 6: Repository Interface (Application Layer)

```csharp
// File: src/Modules/{Module}/UniHub.{Module}.Application/Abstractions/I{Entity}Repository.cs
using UniHub.{Module}.Domain.Entities;

namespace UniHub.{Module}.Application.Abstractions;

public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync({Entity}Id id, CancellationToken ct = default);
    Task<List<{Entity}>> GetAllAsync(CancellationToken ct = default);
    void Add({Entity} entity);
    void Update({Entity} entity);
    void Remove({Entity} entity);
}
```

### Template 7: Repository Implementation (Infrastructure Layer)

```csharp
// File: src/Modules/{Module}/UniHub.{Module}.Infrastructure/Repositories/{Entity}Repository.cs
using Microsoft.EntityFrameworkCore;
using UniHub.{Module}.Application.Abstractions;
using UniHub.{Module}.Domain.Entities;

namespace UniHub.{Module}.Infrastructure.Repositories;

internal sealed class {Entity}Repository : I{Entity}Repository
{
    private readonly {Module}DbContext _context;

    public {Entity}Repository({Module}DbContext context)
    {
        _context = context;
    }

    public async Task<{Entity}?> GetByIdAsync(
        {Entity}Id id,
        CancellationToken ct = default)
    {
        return await _context.{Entities}
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<{Entity}>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.{Entities}.ToListAsync(ct);
    }

    public void Add({Entity} entity) => _context.{Entities}.Add(entity);

    public void Update({Entity} entity) => _context.{Entities}.Update(entity);

    public void Remove({Entity} entity) => _context.{Entities}.Remove(entity);
}
```

### Template 8: API Controller (Presentation Layer)

```csharp
// File: src/Modules/{Module}/UniHub.{Module}.Presentation/Controllers/{Entities}Controller.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniHub.{Module}.Application.Commands.{Command};
using UniHub.{Module}.Application.Queries.{Query};

namespace UniHub.{Module}.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class {Entities}Controller : ControllerBase
{
    private readonly ISender _sender;

    public {Entities}Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        {Command}Request request,
        CancellationToken ct)
    {
        var command = new {Command}Command(request.Property1, request.Property2);
        var result = await _sender.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new {Query}Query(id);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Error);
    }
}
```

### Template 9: React Component (Frontend)

```tsx
// File: frontend/src/components/{component}/{Component}.tsx
'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

interface {Component}Props {
  title: string;
  onAction?: () => void;
}

export function {Component}({ title, onAction }: {Component}Props) {
  const [isLoading, setIsLoading] = useState(false);

  const handleClick = async () => {
    setIsLoading(true);
    try {
      onAction?.();
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <Button onClick={handleClick} disabled={isLoading}>
          {isLoading ? 'Loading...' : 'Click me'}
        </Button>
      </CardContent>
    </Card>
  );
}
```

### Template 10: Custom Hook (Frontend)

```tsx
// File: frontend/src/hooks/use{Entity}.ts
'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api-client';
import type { {Entity}, Create{Entity}Request } from '@/types/{entity}';

export function use{Entities}() {
  return useQuery({
    queryKey: ['{entities}'],
    queryFn: () => api.get<{Entity}[]>('/api/{entities}'),
  });
}

export function use{Entity}(id: string) {
  return useQuery({
    queryKey: ['{entities}', id],
    queryFn: () => api.get<{Entity}>(`/api/{entities}/${id}`),
    enabled: !!id,
  });
}

export function useCreate{Entity}() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: Create{Entity}Request) =>
      api.post<{Entity}>('/api/{entities}', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['{entities}'] });
    },
  });
}
```

---

## ⚠️ ERROR HANDLING

### Nếu build fail:

```bash
# 1. Xem lỗi chi tiết
dotnet build --verbosity detailed

# 2. Kiểm tra references
dotnet list reference

# 3. Restore packages
dotnet restore
```

### Nếu test fail:

```bash
# 1. Chạy test với log
dotnet test --logger "console;verbosity=detailed"

# 2. Chạy test cụ thể
dotnet test --filter "FullyQualifiedName~TestClassName"
```

### Nếu merge conflict:

```bash
# 1. Fetch latest
git fetch origin

# 2. Rebase develop
git rebase origin/develop

# 3. Resolve conflicts manually
# 4. Continue rebase
git rebase --continue
```

---

## ✅ CHECKLIST TRƯỚC KHI ĐÁNH DẤU TASK COMPLETED

- [ ] Tất cả Acceptance Criteria đã được check ✅
- [ ] Code compiles without errors
- [ ] Code follows RULES.md conventions
- [ ] Tests pass (if applicable)
- [ ] Commit message matches format in task
- [ ] PR created và merged
- [ ] Task status updated trong phase file

---

## 🎯 TIPS CHO SONNET 4.5

1. **Đọc toàn bộ context trước** - Đừng implement ngay khi chưa hiểu rõ
2. **Implement từng acceptance criteria một** - Không làm tất cả cùng lúc
3. **Verify sau mỗi step** - `dotnet build` sau mỗi thay đổi lớn
4. **Copy chính xác template** - Thay thế `{placeholder}` bằng giá trị thực
5. **Khi không chắc chắn** - Hỏi user hoặc đọc lại RULES.md
6. **Commit thường xuyên** - Không để quá nhiều changes trong 1 commit

---

## 📞 KHI CẦN HỖ TRỢ

Nếu gặp vấn đề không thể tự giải quyết:

1. **Mô tả rõ lỗi** - Copy full error message
2. **Nêu context** - Đang làm task nào, step nào
3. **Đã thử gì** - Liệt kê các cách đã thử
4. **Yêu cầu cụ thể** - Cần agent làm gì tiếp

---

_Document này được thiết kế để Claude Sonnet 4.5 có thể implement tasks một cách độc lập và chính xác._
