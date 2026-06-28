# 🔧 PHASE 2: CORE INFRASTRUCTURE

> **Xây dựng Shared Kernel và các thành phần cross-cutting**

---

## 📋 PHASE INFO

| Property          | Value               |
| ----------------- | ------------------- |
| **Phase**         | 2                   |
| **Name**          | Core Infrastructure |
| **Status**        | 🔄 IN_PROGRESS      |
| **Progress**      | 9/12 tasks          |
| **Est. Duration** | 2 weeks             |
| **Dependencies**  | Phase 0, Phase 1    |

---

## 🎯 OBJECTIVES

- [x] Xây dựng DDD base classes (Entity, ValueObject, AggregateRoot)
- [x] Implement Domain Events infrastructure
- [x] Setup CQRS với MediatR
- [x] Implement Unit of Work và Repository pattern
- [x] Configure databases (PostgreSQL, MongoDB, Redis)
- [x] Setup Error handling và Logging

---

## 📝 TASKS

### TASK-014: Create Base Entity and Value Object

| Property         | Value                           |
| ---------------- | ------------------------------- |
| **ID**           | TASK-014                        |
| **Status**       | ✅ COMPLETED                    |
| **Priority**     | 🔴 Critical                     |
| **Estimate**     | 3 hours                         |
| **Branch**       | `feature/TASK-014-base-classes` |
| **Dependencies** | Phase 0, Phase 1                |

**Description:**
Tạo base classes cho DDD: Entity, ValueObject, AggregateRoot.

**Acceptance Criteria:**

- [x] `Entity<TId>` base class created
- [x] `ValueObject` base class created
- [x] `AggregateRoot<TId>` base class created
- [x] `IHasDomainEvents` interface created
- [x] Strong-typed ID base created
- [x] Unit tests written

**Files to Create:**

```
src/Shared/UniHub.SharedKernel/
├── Domain/
│   ├── Entity.cs
│   ├── ValueObject.cs
│   ├── AggregateRoot.cs
│   ├── IHasDomainEvents.cs
│   ├── IDomainEvent.cs
│   └── StronglyTypedId.cs
```

**Code Example:**

```csharp
// Entity.cs
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity() { }
    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> entity && Id.Equals(entity.Id);

    public bool Equals(Entity<TId>? other) =>
        other is not null && Id.Equals(other.Id);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !Equals(left, right);
}

// AggregateRoot.cs
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// ValueObject.cs
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        obj is ValueObject other && ValuesAreEqual(other);

    public bool Equals(ValueObject? other) =>
        other is not null && ValuesAreEqual(other);

    private bool ValuesAreEqual(ValueObject other) =>
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(default(int), HashCode.Combine);
}
```

**Commit Message:**

```
feat(shared): add DDD base classes

- Add Entity<TId> base class
- Add ValueObject base class
- Add AggregateRoot<TId> with domain events
- Add IHasDomainEvents interface
- Add IDomainEvent interface
- Add unit tests

Refs: TASK-014
```

---

### TASK-015: Implement Domain Events Infrastructure

| Property         | Value                            |
| ---------------- | -------------------------------- |
| **ID**           | TASK-015                         |
| **Status**       | ✅ COMPLETED                     |
| **Priority**     | 🔴 Critical                      |
| **Estimate**     | 3 hours                          |
| **Branch**       | `feature/TASK-015-domain-events` |
| **Dependencies** | TASK-014                         |

**Description:**
Implement infrastructure để dispatch và handle domain events.

**Acceptance Criteria:**

- [x] `IDomainEvent` interface defined
- [x] `IDomainEventHandler<T>` interface defined
- [x] `DomainEventDispatcher` implemented
- [x] Integration với MediatR
- [x] Unit tests written

**Files to Create:**

```
src/Shared/UniHub.SharedKernel/
├── Events/
│   ├── IDomainEvent.cs
│   ├── IDomainEventHandler.cs
│   └── DomainEventDispatcher.cs
```

**Commit Message:**

```
feat(shared): implement domain events infrastructure

- Add IDomainEvent interface
- Add IDomainEventHandler interface
- Add DomainEventDispatcher
- Integrate with MediatR INotification
- Add unit tests

Refs: TASK-015
```

---

### TASK-016: Setup CQRS Base Infrastructure

| Property         | Value                         |
| ---------------- | ----------------------------- |
| **ID**           | TASK-016                      |
| **Status**       | ✅ COMPLETED                  |
| **Priority**     | 🔴 Critical                   |
| **Estimate**     | 4 hours                       |
| **Branch**       | `feature/TASK-016-cqrs-setup` |
| **Dependencies** | TASK-014                      |

**Description:**
Setup CQRS pattern với ICommand, IQuery, và handlers.

**Acceptance Criteria:**

- [x] `ICommand<TResult>` interface created
- [x] `ICommandHandler<TCommand, TResult>` interface created
- [x] `IQuery<TResult>` interface created
- [x] `IQueryHandler<TQuery, TResult>` interface created
- [x] MediatR integration configured
- [x] Unit tests written

**Files to Create:**

```
src/Shared/UniHub.SharedKernel/
├── CQRS/
│   ├── ICommand.cs
│   ├── ICommandHandler.cs
│   ├── IQuery.cs
│   └── IQueryHandler.cs
```

**Code Example:**

```csharp
// ICommand.cs
public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

// ICommandHandler.cs
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }

public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }

// IQuery.cs
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

// IQueryHandler.cs
public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
```

**Commit Message:**

```
feat(shared): setup CQRS base infrastructure

- Add ICommand and ICommandHandler interfaces
- Add IQuery and IQueryHandler interfaces
- Integrate with MediatR IRequest
- Configure Result pattern integration
- Add unit tests

Refs: TASK-016
```

---

### TASK-017: Setup MediatR Pipeline Behaviors

| Property         | Value                                 |
| ---------------- | ------------------------------------- |
| **ID**           | TASK-017                              |
| **Status**       | ✅ COMPLETED                          |
| **Priority**     | 🔴 Critical                           |
| **Estimate**     | 4 hours                               |
| **Branch**       | `feature/TASK-017-pipeline-behaviors` |
| **Dependencies** | TASK-016                              |

**Description:**
Implement MediatR pipeline behaviors cho validation, logging, performance.

**Acceptance Criteria:**

- [x] `ValidationBehavior` implemented
- [x] `LoggingBehavior` implemented
- [x] `PerformanceBehavior` implemented
- [x] `UnhandledExceptionBehavior` implemented
- [x] Behaviors registered in DI
- [x] Unit tests written

**Files to Create:**

```
src/Shared/UniHub.Infrastructure/
├── Behaviors/
│   ├── ValidationBehavior.cs
│   ├── LoggingBehavior.cs
│   ├── PerformanceBehavior.cs
│   └── UnhandledExceptionBehavior.cs
```

**Commit Message:**

```
feat(infra): implement MediatR pipeline behaviors

- Add ValidationBehavior with FluentValidation
- Add LoggingBehavior for request/response logging
- Add PerformanceBehavior for slow query detection
- Add UnhandledExceptionBehavior
- Register in DI container

Refs: TASK-017
```

---

### TASK-018: Implement Unit of Work Pattern

| Property         | Value                           |
| ---------------- | ------------------------------- |
| **ID**           | TASK-018                        |
| **Status**       | ✅ COMPLETED                    |
| **Priority**     | 🔴 Critical                     |
| **Estimate**     | 3 hours                         |
| **Branch**       | `feature/TASK-018-unit-of-work` |
| **Dependencies** | TASK-014                        |

**Description:**
Implement Unit of Work pattern cho transaction management.

**Acceptance Criteria:**

- [x] `IUnitOfWork` interface defined
- [x] `UnitOfWork` implementation cho EF Core
- [x] Domain events dispatched on SaveChanges
- [x] Transaction support
- [x] Unit tests written

**Files to Create:**

```
src/Shared/UniHub.SharedKernel/
├── Persistence/
│   └── IUnitOfWork.cs

src/Shared/UniHub.Infrastructure/
├── Persistence/
│   └── UnitOfWork.cs
```

**Code Example:**

```csharp
// IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

**Commit Message:**

```
feat(shared): implement Unit of Work pattern

- Add IUnitOfWork interface
- Add UnitOfWork implementation for EF Core
- Dispatch domain events on SaveChanges
- Add transaction support
- Add unit tests

Refs: TASK-018
```

---

### TASK-019: Implement Repository Base

| Property         | Value                              |
| ---------------- | ---------------------------------- |
| **ID**           | TASK-019                           |
| **Status**       | ✅ COMPLETED                       |
| **Priority**     | 🔴 Critical                        |
| **Estimate**     | 3 hours                            |
| **Branch**       | `feature/TASK-019-repository-base` |
| **Dependencies** | TASK-018                           |

**Description:**
Implement base repository cho Aggregate Roots.

**Acceptance Criteria:**

- [x] `IRepository<T>` interface defined
- [x] `Repository<T>` base implementation
- [x] Specification pattern support
- [x] Unit tests written

**Files to Create:**

```
src/Shared/UniHub.SharedKernel/
├── Persistence/
│   ├── IRepository.cs
│   └── Specification.cs

src/Shared/UniHub.Infrastructure/
├── Persistence/
│   └── Repository.cs
```

**Commit Message:**

```
feat(shared): implement repository base

- Add IRepository<T> interface
- Add Repository<T> base implementation
- Add Specification pattern support
- Add unit tests

Refs: TASK-019
```

---

### TASK-020: Setup PostgreSQL DbContext

| Property         | Value                             |
| ---------------- | --------------------------------- |
| **ID**           | TASK-020                          |
| **Status**       | ✅ COMPLETED                      |
| **Priority**     | 🔴 Critical                       |
| **Estimate**     | 3 hours                           |
| **Branch**       | `feature/TASK-020-postgres-setup` |
| **Dependencies** | TASK-019                          |

**Description:**
Configure Entity Framework Core với PostgreSQL.

**Acceptance Criteria:**

- [ ] `ApplicationDbContext` created
- [ ] Connection string configured
- [ ] Interceptors for audit fields
- [ ] Migrations configured
- [ ] Health check added

**Files to Create:**

```
src/Shared/UniHub.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   └── Interceptors/
│       ├── AuditableEntityInterceptor.cs
│       └── DomainEventInterceptor.cs
```

**Commit Message:**

```
feat(infra): setup PostgreSQL with EF Core

- Add ApplicationDbContext
- Configure connection string
- Add AuditableEntityInterceptor
- Add DomainEventInterceptor
- Add health check

Refs: TASK-020
```

---

### TASK-021: Setup MongoDB Context

| Property         | Value                            |
| ---------------- | -------------------------------- |
| **ID**           | TASK-021                         |
| **Status**       | ✅ COMPLETED                     |
| **Priority**     | 🔴 Critical                      |
| **Estimate**     | 3 hours                          |
| **Branch**       | `feature/TASK-021-mongodb-setup` |
| **Dependencies** | TASK-007                         |

**Description:**
Configure MongoDB.Driver cho document storage.

**Acceptance Criteria:**

- [ ] `MongoDbContext` created
- [ ] Connection string configured
- [ ] Collection mappings configured
- [ ] Indexes configured
- [ ] Health check added

**Files to Create:**

```
src/Shared/UniHub.Infrastructure/
├── MongoDb/
│   ├── MongoDbContext.cs
│   ├── MongoDbSettings.cs
│   └── Collections/
```

**Commit Message:**

```
feat(infra): setup MongoDB context

- Add MongoDbContext
- Configure connection string
- Add collection mappings
- Configure indexes
- Add health check

Refs: TASK-021
```

---

### TASK-022: Setup Redis Caching ✅

| Property         | Value                          |
| ---------------- | ------------------------------ |
| **ID**           | TASK-022                       |
| **Status**       | ✅ COMPLETED                   |
| **Priority**     | 🟡 Medium                      |
| **Estimate**     | 3 hours                        |
| **Branch**       | `feature/TASK-022-redis-setup` |
| **Dependencies** | TASK-007                       |

**Description:**
Configure Redis cho caching và SignalR backplane.

**Acceptance Criteria:**

- [x] `ICacheService` interface defined
- [x] `RedisCacheService` implementation
- [x] Connection string configured
- [x] SignalR backplane configured
- [x] Health check added

**Files to Create:**

```
src/Shared/UniHub.Infrastructure/
├── Caching/
│   ├── ICacheService.cs
│   ├── RedisCacheService.cs
│   └── CacheKeys.cs
```

**Implementation Details:**

- **ICacheService.cs**: Generic caching interface with methods:
  - `GetAsync<T>`: Retrieve cached value
  - `SetAsync<T>`: Store value with optional expiration
  - `RemoveAsync`: Delete single key
  - `RemoveByPatternAsync`: Delete keys matching pattern
  - `GetOrSetAsync<T>`: Cache-aside pattern implementation
- **RedisCacheService.cs**: Redis implementation using `IDistributedCache` and `IConnectionMultiplexer`
  - Uses byte arrays for serialization (base interface methods)
  - Default expiration: 1 hour
  - Pattern-based key removal via `server.Keys()`
- **CacheKeys.cs**: Static helper class with key prefixes for all modules (user, post, forum, notification, course, chat)
- **DependencyInjection.cs**: Added `AddRedisCache` method with:
  - IConnectionMultiplexer configuration (AbortOnConnectFail=false, timeouts=5000ms)
  - StackExchangeRedisCache with "UniHub:" instance name
  - SignalR Redis backplane configuration
  - Health check registration
- **Unit Tests**: 17 tests (10 for RedisCacheService, 7 for CacheKeys) - 148 total passing

**Commit Message:**

```
feat: implement Redis caching infrastructure (TASK-022)

- Add ICacheService interface with Get, Set, Remove, RemoveByPattern, GetOrSet methods
- Implement RedisCacheService using IDistributedCache and IConnectionMultiplexer
- Add CacheKeys helper class with key prefixes for all modules
- Configure Redis connection with health check and SignalR backplane
- Update appsettings with Redis connection strings
- Add 17 unit tests for caching functionality (148 tests total passing)

Refs: TASK-022
```

---

### TASK-023: Implement Result Pattern

| Property         | Value                             |
| ---------------- | --------------------------------- |
| **ID**           | TASK-023                          |
| **Status**       | ✅ COMPLETED                      |
| **Priority**     | 🔴 Critical                       |
| **Estimate**     | 2 hours                           |
| **Branch**       | `feature/TASK-023-result-pattern` |
| **Dependencies** | None                              |

**Description:**
Implement Result pattern cho error handling functional style.

**Acceptance Criteria:**

- [x] `Result` class implemented
- [x] `Result<T>` class implemented
- [x] `Error` class implemented
- [x] Extension methods for mapping
- [x] Unit tests written

**Files to Create:**

```
src/Shared/UniHub.SharedKernel/
├── Results/
│   ├── Result.cs
│   ├── ResultT.cs
│   ├── Error.cs
│   └── ResultExtensions.cs
```

**Code Example:**

```csharp
// Result.cs
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

// Error.cs
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");
}
```

**Commit Message:**

```
feat(shared): implement Result pattern

- Add Result class for operation results
- Add Result<T> for typed results
- Add Error record for error handling
- Add extension methods
- Add unit tests

Refs: TASK-023
```

---

### TASK-024: Setup Serilog Logging ✅

| Property         | Value                            |
| ---------------- | -------------------------------- |
| **ID**           | TASK-024                         |
| **Status**       | ✅ COMPLETED                     |
| **Priority**     | 🟡 Medium                        |
| **Estimate**     | 2 hours                          |
| **Branch**       | `feature/TASK-024-serilog-setup` |
| **Dependencies** | TASK-002                         |

**Description:**
Configure Serilog cho structured logging.

**Acceptance Criteria:**

- [x] Serilog configured in Program.cs
- [x] Console sink configured
- [x] File sink configured (dev)
- [x] Enrichers configured
- [x] Request logging middleware

**Implementation Details:**

- **Program.cs**:
  - Bootstrap logger configured before WebApplication builder
  - Reads from appsettings.json and environment-specific files
  - Console sink with custom output template
  - File sink with daily rolling logs (30 days retention)
  - Try-catch-finally for proper cleanup and error handling
- **Enrichers**:
  - FromLogContext: Contextual logging
  - WithMachineName: Machine identification
  - WithThreadId: Thread tracking
  - WithProcessId: Process tracking
- **Request Logging**:
  - UseSerilogRequestLogging middleware
  - Custom message template with timing
  - Diagnostic context enrichment (RequestHost, RequestScheme, UserAgent, RemoteIP)
- **Configuration**:
  - appsettings.json: Base configuration with Information level
  - appsettings.Development.json: Debug level with more detailed logging
  - Minimum level overrides for Microsoft and System namespaces
- **Packages**:
  - Serilog.AspNetCore 8.0.3
  - Serilog.Sinks.Console 6.0.0
  - Serilog.Sinks.File 6.0.0
  - Serilog.Enrichers.Environment 3.0.1
  - Serilog.Enrichers.Thread 4.0.0
  - Serilog.Enrichers.Process 3.0.0

**Commit Message:**

```
feat: setup Serilog logging (TASK-024)

- Configure Serilog in Program.cs with console and file sinks
- Add enrichers (machine name, thread ID, process ID)
- Add request logging middleware with enhanced diagnostic context
- Update appsettings.json with Serilog configuration
- Add Serilog packages: AspNetCore, Sinks.Console, Sinks.File, Enrichers
- All 148 tests passing

Refs: TASK-024
```

---

### TASK-025: Setup Global Exception Handling ✅

| Property         | Value                                 |
| ---------------- | ------------------------------------- |
| **ID**           | TASK-025                              |
| **Status**       | ✅ COMPLETED                          |
| **Priority**     | 🔴 Critical                           |
| **Estimate**     | 2 hours                               |
| **Branch**       | `feature/TASK-025-exception-handling` |
| **Dependencies** | TASK-023, TASK-024                    |

**Description:**
Implement global exception handling middleware.

**Acceptance Criteria:**

- [x] `GlobalExceptionHandler` middleware created
- [x] Custom exception types defined
- [x] ProblemDetails response format
- [x] Error logging
- [x] Development vs Production error details

**Files to Create:**

```
src/UniHub.API/
├── Middlewares/
│   └── GlobalExceptionHandler.cs

src/Shared/UniHub.SharedKernel/
├── Exceptions/
│   ├── DomainException.cs
│   ├── NotFoundException.cs
│   ├── ValidationException.cs
│   └── UnauthorizedException.cs
```

**Implementation Details:**

- **Custom Exception Types**:
  - `DomainException`: For domain rule violations with optional name property
  - `NotFoundException`: For entity not found errors with EntityName and Key properties
  - `ValidationException`: For validation failures with structured error dictionary
  - `UnauthorizedException`: For unauthorized access attempts
- **GlobalExceptionHandler Middleware**:
  - Implements `IExceptionHandler` interface
  - Maps exceptions to appropriate HTTP status codes (400, 401, 404, 500)
  - Returns RFC 9110 compliant ProblemDetails responses
  - Includes trace ID and timestamp in all responses
  - Logs all exceptions with structured logging
  - Environment-aware error details:
    - Production: Only status code, title, and trace ID
    - Development: Includes exception message, type, and stack trace
- **Exception Mapping**:
  - ValidationException → 400 Bad Request
  - DomainException → 400 Bad Request
  - NotFoundException → 404 Not Found
  - UnauthorizedException → 401 Unauthorized
  - Other exceptions → 500 Internal Server Error
- **Registration**:
  - Added `AddExceptionHandler<GlobalExceptionHandler>()` to DI
  - Added `AddProblemDetails()` for RFC compliance
  - Added `UseExceptionHandler()` middleware in pipeline
  - Added project reference from API to SharedKernel
- **Unit Tests**: 15 tests for exception types (4 tests each for DomainException, NotFoundException, ValidationException, UnauthorizedException, plus constructors)

**Commit Message:**

```
feat: setup global exception handling (TASK-025)

- Add custom exception types: DomainException, NotFoundException, ValidationException, UnauthorizedException
- Implement GlobalExceptionHandler middleware with ProblemDetails response
- Add exception mapping to HTTP status codes
- Add error logging with trace ID
- Add environment-specific error details (dev vs production)
- Register exception handler and ProblemDetails in DI
- Add 15 unit tests for exception types (163 tests total passing)

Refs: TASK-025
```

---

## ✅ COMPLETION CHECKLIST

- [x] TASK-014: Create Base Entity and Value Object
- [x] TASK-015: Implement Domain Events Infrastructure
- [x] TASK-016: Setup CQRS Base Infrastructure
- [x] TASK-017: Setup MediatR Pipeline Behaviors
- [x] TASK-018: Implement Unit of Work Pattern
- [x] TASK-019: Create Repository Base
- [x] TASK-020: Setup PostgreSQL DbContext
- [x] TASK-021: Setup MongoDB Context
- [x] TASK-022: Setup Redis Caching
- [x] TASK-023: Implement Result Pattern
- [x] TASK-024: Setup Serilog Logging
- [x] TASK-025: Setup Global Exception Handling

---

## 📝 NOTES

- Phase này là foundation cho tất cả modules
- Phải có unit tests cho mỗi component
- Code phải follow RULES.md strictly

---

_Last Updated: 2026-02-04_
