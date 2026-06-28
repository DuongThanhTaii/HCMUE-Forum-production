# TASK-107: Entity Configuration Fixes Guide

> **Hướng dẫn sửa lỗi Entity Configurations để EF Core Migrations chạy thành công**

---

## 📋 TABLE OF CONTENTS

1. [Tổng quan](#tổng-quan)
2. [Issues đã phát hiện](#issues-đã-phát-hiện)
3. [Patterns & Best Practices](#patterns--best-practices)
4. [Fix Checklist](#fix-checklist)
5. [Testing Strategy](#testing-strategy)
6. [References](#references)

---

## 🎯 TỔNG QUAN

### Vấn đề hiện tại

EF Core migration generation thất bại do entity configurations không tuân thủ các quy tắc:

- Shadow properties phải được define TRƯỚC khi sử dụng trong `HasKey()`
- `Navigation()` cho primitive collections gây lỗi
- Owned entities với composite keys cần shadow property definitions
- Unit tests sử dụng `TestDbContext` thay vì `ApplicationDbContext`

### Commit hiện tại

**Branch**: `feature/TASK-107-migration-seed`  
**Commit**: `c1de8c6` - WIP với các fixes ban đầu

### Mục tiêu

Sửa tất cả entity configurations để:

- ✅ `dotnet ef migrations add InitialCreate` chạy thành công
- ✅ Migration file được generate với đầy đủ tables & relationships
- ✅ Không có warnings về shadow properties hoặc navigation issues
- ✅ Build thành công (0 errors)

---

## 🐛 ISSUES ĐÃ PHÁT HIỆN

### 1. Shadow Property Definition Order ⚠️ CRITICAL

**Error Message:**

```
The property 'user_id' cannot be added to the type 'Vote' because no property type was specified and there is no corresponding CLR property or field. To add a shadow state property, the property type must be specified.
```

**Root Cause:**  
Shadow properties phải được định nghĩa với type TRƯỚC khi sử dụng trong `HasKey()`, `HasForeignKey()`, hoặc các method khác.

**❌ Sai:**

```csharp
builder.OwnsMany(m => m.Votes, vote =>
{
    vote.HasKey("message_id", "user_id");  // ❌ Lỗi: user_id chưa được define

    vote.Property(v => v.UserId)
        .HasColumnName("user_id");
});
```

**✅ Đúng:**

```csharp
builder.OwnsMany(m => m.Votes, vote =>
{
    // 1. Define shadow property FIRST
    vote.Property<Guid>("message_id")
        .HasColumnName("message_id")
        .IsRequired();

    // 2. Define regular properties
    vote.Property(v => v.UserId)
        .HasColumnName("user_id")
        .IsRequired();

    // 3. NOW configure relationships & keys
    vote.WithOwner().HasForeignKey("message_id");
    vote.HasKey("message_id", nameof(Vote.UserId));
});
```

**Đã fix:** PostConfiguration, CommentConfiguration, MessageConfiguration

---

### 2. Navigation() Calls for Primitive Collections ⚠️ CRITICAL

**Error Message:**

```
Navigation 'Category.ModeratorIds' was not found. Add the navigation to the entity type using 'HasOne', 'HasMany', or 'OwnsOne'/'OwnsMany' methods before configuring it.
```

**Root Cause:**  
EF Core 10 không hỗ trợ `.Navigation()` cho primitive collections (List<Guid>, List<string>). Chỉ dùng `.Property()` với backing field.

**❌ Sai:**

```csharp
builder.Property("_moderatorIds")
    .HasColumnName("moderator_ids")
    .HasColumnType("jsonb");

builder.Navigation(c => c.ModeratorIds)          // ❌ EF Core Error
    .UsePropertyAccessMode(PropertyAccessMode.Field);
```

**✅ Đúng:**

```csharp
// Chỉ cần define property với backing field
builder.Property("_moderatorIds")
    .HasColumnName("moderator_ids")
    .HasColumnType("jsonb");

// Không cần Navigation() - EF Core tự động map qua backing field
```

**Đã fix:** CategoryConfiguration, CourseConfiguration, CompanyConfiguration, JobPostingConfiguration, ChannelConfiguration, ConversationConfiguration

---

### 3. Owned Collections với Composite Keys

**Pattern cần tuân thủ cho MỌI owned collections:**

```csharp
builder.OwnsMany(parent => parent.Children, child =>
{
    child.ToTable("children_table", "schema");

    // ✅ STEP 1: Define ALL shadow properties FIRST
    child.Property<Guid>("parent_id")
        .HasColumnName("parent_id")
        .IsRequired();

    child.Property<Guid>("other_shadow_prop")
        .HasColumnName("other_shadow_prop")
        .IsRequired();

    // ✅ STEP 2: Define regular CLR properties
    child.Property(c => c.Name)
        .HasColumnName("name")
        .HasMaxLength(100)
        .IsRequired();

    // ✅ STEP 3: Configure relationships
    child.WithOwner().HasForeignKey("parent_id");

    // ✅ STEP 4: Configure composite key (shadow + CLR properties)
    child.HasKey("parent_id", nameof(ChildEntity.Name));
});
```

---

### 4. Unit Tests Issue

**Error:**

```csharp
// UnitOfWorkTests.cs line 26
error CS1503: Argument 1: cannot convert from 'TestDbContext' to 'ApplicationDbContext'
```

**Root Cause:**  
Sau khi đổi `UnitOfWork` constructor từ `DbContext` → `ApplicationDbContext`, các tests cần refactor.

**Solution Options:**

**Option A: Sử dụng ApplicationDbContext (Recommended)**

```csharp
public class UnitOfWorkTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _publisherMock = new Mock<IPublisher>();
        _unitOfWork = new UnitOfWork(_context, _publisherMock.Object);
    }

    // Tests cần dùng actual entities thay vì TestEntity
    [Fact]
    public async Task SaveChangesAsync_ShouldSaveChangesToDatabase()
    {
        // Use real entity from domain (e.g., Category, User)
        var category = Category.Create(
            CategoryName.Create("Test").Value,
            CategoryDescription.Create("Test Desc").Value
        ).Value;

        _context.Categories.Add(category);
        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().Be(1);
    }
}
```

**Option B: Refactor UnitOfWork để accept DbContext (Not Recommended)**

- Giữ nguyên tests nhưng rollback UnitOfWork change
- ❌ Không type-safe, không leverage ApplicationDbContext-specific features

---

## 📐 PATTERNS & BEST PRACTICES

### Configuration Template

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Module.Domain.Entities;

namespace UniHub.Module.Infrastructure.Persistence.Configurations;

public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        // 1. TABLE & SCHEMA
        builder.ToTable("entities", "schema_name");

        // 2. PRIMARY KEY
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => EntityId.Create(value))
            .HasColumnName("id");

        // 3. OWNED VALUE OBJECTS (single)
        builder.OwnsOne(e => e.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            name.HasIndex(n => n.Value).IsUnique(); // If unique
        });

        // 4. SIMPLE PROPERTIES
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<int>()  // For enums
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // 5. PRIMITIVE COLLECTIONS (backing field only, NO Navigation)
        builder.Property("_tags")
            .HasColumnName("tags")
            .HasColumnType("jsonb");

        // 6. OWNED COLLECTIONS
        builder.OwnsMany(e => e.Children, child =>
        {
            child.ToTable("entity_children", "schema_name");

            // Shadow properties FIRST
            child.Property<Guid>("entity_id")
                .HasColumnName("entity_id")
                .IsRequired();

            // Regular properties
            child.Property(c => c.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            // Relationships & Keys LAST
            child.WithOwner().HasForeignKey("entity_id");
            child.HasKey("entity_id", nameof(Child.Name));
        });

        // 7. INDEXES
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);

        // 8. IGNORE DOMAIN EVENTS
        builder.Ignore(e => e.DomainEvents);
    }
}
```

### Naming Conventions

| Element         | Convention                  | Example                               |
| --------------- | --------------------------- | ------------------------------------- |
| Table Name      | snake_case plural           | `users`, `post_votes`                 |
| Schema Name     | lowercase singular          | `identity`, `forum`, `chat`           |
| Column Name     | snake_case                  | `user_id`, `created_at`, `vote_score` |
| Shadow Property | snake_case (same as column) | `"message_id"`, `"parent_id"`         |
| Backing Field   | \_camelCase                 | `"_tags"`, `"_moderatorIds"`          |

---

## ✅ FIX CHECKLIST

### Module 1: Identity ✅ (Already Fixed in TASK-101)

- [x] UserConfiguration
- [x] RoleConfiguration
- [x] PermissionConfiguration
- [x] RefreshTokenConfiguration
- [x] PasswordResetTokenConfiguration

### Module 2: Forum ⚠️ (Partially Fixed)

- [x] PostConfiguration - Fixed Vote shadow properties
- [x] CommentConfiguration - Fixed Vote shadow properties
- [x] CategoryConfiguration - Removed Navigation()
- [ ] **TagConfiguration** - ❌ NEEDS REVIEW
- [ ] **BookmarkConfiguration** - ❌ NEEDS REVIEW
- [ ] **ReportConfiguration** - ❌ NEEDS REVIEW

### Module 3: Learning ⚠️ (Partially Fixed)

- [x] CourseConfiguration - Removed Navigation()
- [ ] **DocumentConfiguration** - ❌ NEEDS REVIEW (có FileMetadata owned object)
- [ ] **FacultyConfiguration** - ❌ NEEDS REVIEW

### Module 4: Chat ⚠️ (Partially Fixed)

- [x] ConversationConfiguration - Removed Navigation()
- [x] MessageConfiguration - Fixed Attachments shadow property
- [x] ChannelConfiguration - Removed Navigation()

### Module 5: Career ⚠️ (Partially Fixed)

- [x] CompanyConfiguration - Removed Navigation()
- [x] JobPostingConfiguration - Removed Navigation()
- [ ] **ApplicationConfiguration** - ❌ NEEDS REVIEW
- [ ] **RecruiterConfiguration** - ❌ NEEDS REVIEW

### Module 6: Notification ❌ (Not Reviewed)

- [ ] **NotificationConfiguration** - ❌ NEEDS REVIEW (có Metadata owned object phức tạp)
- [ ] **NotificationPreferenceConfiguration** - ❌ NEEDS REVIEW
- [ ] **NotificationTemplateConfiguration** - ❌ NEEDS REVIEW

### Tests

- [ ] **UnitOfWorkTests.cs** - ❌ Cần refactor để dùng ApplicationDbContext

---

## 🔧 TESTING STRATEGY

### Step 1: Incremental Module Testing

Test từng module một để isolate issues:

```bash
# Test specific module configurations
dotnet ef migrations add TestIdentity \
  --project src/Shared/UniHub.Infrastructure \
  --startup-project src/UniHub.API \
  --output-dir Persistence/Migrations/Test \
  --context ApplicationDbContext

# If fails, check error message for specific Configuration
```

### Step 2: Validate Configuration Syntax

Tạo helper method để validate từng configuration:

```csharp
// Add to ApplicationDbContextFactory for testing
public void ValidateConfigurations()
{
    using var context = CreateDbContext(Array.Empty<string>());

    try
    {
        _ = context.Model; // Force model building
        Console.WriteLine("✅ All configurations valid");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Configuration error: {ex.Message}");
        throw;
    }
}
```

### Step 3: Migration Diff Check

Sau khi generate migration thành công:

```bash
# Review migration file
cat src/Shared/UniHub.Infrastructure/Persistence/Migrations/*_InitialCreate.cs

# Check for:
# - All expected tables present
# - Correct column types (jsonb for collections)
# - Foreign keys configured
# - Indexes created
```

### Step 4: Apply & Rollback Test

```bash
# Apply migration to test database
dotnet ef database update \
  --project src/Shared/UniHub.Infrastructure \
  --startup-project src/UniHub.API

# Verify tables created
psql -h localhost -U postgres -d unihub_db -c "\dt"

# Rollback test
dotnet ef database update 0 \
  --project src/Shared/UniHub.Infrastructure \
  --startup-project src/UniHub.API
```

---

## 📚 STEP-BY-STEP FIX WORKFLOW

### Phase 1: Fix Remaining Configurations (Est: 2-3 hours)

1. **Review & Fix Forum Module** (30 min)

   ```bash
   # Check TagConfiguration
   code src/Modules/Forum/UniHub.Forum.Infrastructure/Persistence/Configurations/TagConfiguration.cs

   # Check BookmarkConfiguration
   code src/Modules/Forum/UniHub.Forum.Infrastructure/Persistence/Configurations/BookmarkConfiguration.cs

   # Check ReportConfiguration
   code src/Modules/Forum/UniHub.Forum.Infrastructure/Persistence/Configurations/ReportConfiguration.cs
   ```

2. **Review & Fix Learning Module** (30 min)

   ```bash
   code src/Modules/Learning/UniHub.Learning.Infrastructure/Persistence/Configurations/DocumentConfiguration.cs
   code src/Modules/Learning/UniHub.Learning.Infrastructure/Persistence/Configurations/FacultyConfiguration.cs
   ```

3. **Review & Fix Career Module** (30 min)

   ```bash
   code src/Modules/Career/UniHub.Career.Infrastructure/Persistence/Configurations/ApplicationConfiguration.cs
   code src/Modules/Career/UniHub.Career.Infrastructure/Persistence/Configurations/RecruiterConfiguration.cs
   ```

4. **Review & Fix Notification Module** (45 min) ⚠️ MOST COMPLEX
   ```bash
   code src/Modules/Notification/UniHub.Notification.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs
   # Pay special attention to Metadata owned object
   ```

### Phase 2: Fix Unit Tests (Est: 30 min)

```bash
# Refactor UnitOfWorkTests
code tests/Shared/UniHub.Infrastructure.Tests/Persistence/UnitOfWorkTests.cs

# Update to use ApplicationDbContext
# Replace TestEntity with real domain entities
# Run tests
dotnet test tests/Shared/UniHub.Infrastructure.Tests
```

### Phase 3: Generate Migration (Est: 15 min)

```bash
# Clean previous attempts
rm -rf src/Shared/UniHub.Infrastructure/Persistence/Migrations/*

# Generate fresh migration
dotnet ef migrations add InitialCreate \
  --project src/Shared/UniHub.Infrastructure \
  --startup-project src/UniHub.API \
  --output-dir Persistence/Migrations \
  --verbose

# If successful, review generated files
```

### Phase 4: Verify & Commit (Est: 15 min)

```bash
# Build solution
dotnet build

# Review migration file
code src/Shared/UniHub.Infrastructure/Persistence/Migrations/*_InitialCreate.cs

# Commit
git add -A
git commit -m "feat(TASK-107): Fix all entity configurations for EF Core migrations

- Fixed remaining owned collections shadow properties
- Refactored UnitOfWorkTests to use ApplicationDbContext
- Generated InitialCreate migration successfully
- All configurations follow EF Core 10 best practices

Result: Migration ready for database application"
```

---

## 🎯 SUCCESS CRITERIA

Migration generation thành công khi:

✅ **Command Output:**

```
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

✅ **Files Created:**

- `src/Shared/UniHub.Infrastructure/Persistence/Migrations/{timestamp}_InitialCreate.cs`
- `src/Shared/UniHub.Infrastructure/Persistence/Migrations/{timestamp}_InitialCreate.Designer.cs`
- `src/Shared/UniHub.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`

✅ **Migration Contains:**

- 30+ `migrationBuilder.CreateTable()` calls (one per entity + owned collections)
- Foreign key constraints: `migrationBuilder.AddForeignKey()`
- Indexes: `migrationBuilder.CreateIndex()`
- Jsonb columns for primitive collections

✅ **Build Status:**

- 0 errors
- Warnings acceptable (nullable, XML docs)

---

## 📖 REFERENCES

### EF Core 10 Documentation

- [Owned Entity Types](https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities)
- [Shadow Properties](https://learn.microsoft.com/en-us/ef/core/modeling/shadow-properties)
- [Value Conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)
- [Primitive Collections](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections)

### Project-Specific

- [TASK-101 Implementation](../tasks/phase-9.5.md#task-101-ef-core-entity-configurations)
- [ApplicationDbContext](../../src/Shared/UniHub.Infrastructure/Persistence/ApplicationDbContext.cs)
- [Domain Entities](../../src/Modules/)

### Common EF Core Errors

- [NU1008 - Central Package Management](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1008)
- [Design-time DbContext Creation](https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation)

---

## 💡 TIPS & TRICKS

### Quick Find Shadow Property Issues

```bash
# Search for HasKey with string parameters
grep -r "HasKey(\"" src/Modules/*/Infrastructure/Persistence/Configurations/

# Search for Navigation() calls
grep -r "\.Navigation(" src/Modules/*/Infrastructure/Persistence/Configurations/
```

### Validate One Configuration at a Time

```csharp
// Temporarily comment out in ApplicationDbContext.OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Test specific assembly only
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(ForumConfiguration).Assembly  // Test Forum only
    );
}
```

### Debug Mode for EF Tools

```bash
# Run with verbose logging
dotnet ef migrations add InitialCreate \
  --verbose \
  --project src/Shared/UniHub.Infrastructure \
  --startup-project src/UniHub.API 2>&1 | tee ef-debug.log

# Check log for detailed errors
cat ef-debug.log
```

---

## 📞 NEXT STEPS

After completing this guide:

1. ✅ Tất cả entity configurations fixed
2. ✅ Unit tests passing
3. ✅ Migration generated successfully
4. 🔜 **TASK-107b**: Apply migration to Neon.tech database
5. 🔜 **TASK-107c**: Create seed data classes
6. 🔜 **TASK-107d**: Test end-to-end CRUD operations

**Total Estimated Time**: 3-4 hours for complete TASK-107 implementation

---

_Last Updated: 2026-02-09_  
_Author: GitHub Copilot_  
_Status: Ready for Implementation_
