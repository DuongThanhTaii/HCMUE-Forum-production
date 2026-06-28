using UniHub.Forum.Domain.Categories.ValueObjects;
using UniHub.Forum.Domain.Events;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Categories;

public sealed class Category : AggregateRoot<CategoryId>
{
    private readonly List<Guid> _moderatorIds = new();

    public CategoryName Name { get; private set; }
    public CategoryDescription Description { get; private set; }
    public Slug Slug { get; private set; }
    public CategoryId? ParentCategoryId { get; private set; }
    public int PostCount { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<Guid> ModeratorIds => _moderatorIds.AsReadOnly();

    // EF Core constructor
    private Category()
    {
        Name = null!;
        Description = null!;
        Slug = null!;
    }

    private Category(
        CategoryId id,
        CategoryName name,
        CategoryDescription description,
        Slug slug,
        CategoryId? parentCategoryId,
        int displayOrder)
    {
        Id = id;
        Name = name;
        Description = description;
        Slug = slug;
        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        PostCount = 0;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Category> Create(
        CategoryName name,
        CategoryDescription description,
        CategoryId? parentCategoryId = null,
        int displayOrder = 0)
    {
        var slugResult = Slug.Create(name.Value);
        if (slugResult.IsFailure)
        {
            return Result.Failure<Category>(slugResult.Error);
        }

        var category = new Category(
            CategoryId.CreateUnique(),
            name,
            description,
            slugResult.Value,
            parentCategoryId,
            displayOrder);

        category.AddDomainEvent(new CategoryCreatedEvent(category.Id, category.Name.Value));
        return Result.Success(category);
    }

    public Result Update(CategoryName name, CategoryDescription description, int displayOrder)
    {
        var slugResult = Slug.Create(name.Value);
        if (slugResult.IsFailure)
        {
            return Result.Failure(slugResult.Error);
        }

        Name = name;
        Description = description;
        Slug = slugResult.Value;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CategoryUpdatedEvent(Id));
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
        {
            return Result.Failure(new Error("Category.AlreadyActive", "Category is already active"));
        }

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CategoryActivatedEvent(Id));
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(new Error("Category.AlreadyInactive", "Category is already inactive"));
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CategoryDeactivatedEvent(Id));
        return Result.Success();
    }

    public Result AssignModerator(Guid moderatorId)
    {
        if (_moderatorIds.Contains(moderatorId))
        {
            return Result.Failure(new Error("Category.ModeratorAlreadyAssigned", "Moderator is already assigned to this category"));
        }

        _moderatorIds.Add(moderatorId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ModeratorAssignedToCategoryEvent(Id, moderatorId));
        return Result.Success();
    }

    public Result RemoveModerator(Guid moderatorId)
    {
        if (!_moderatorIds.Contains(moderatorId))
        {
            return Result.Failure(new Error("Category.ModeratorNotAssigned", "Moderator is not assigned to this category"));
        }

        _moderatorIds.Remove(moderatorId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ModeratorRemovedFromCategoryEvent(Id, moderatorId));
        return Result.Success();
    }

    public void IncrementPostCount()
    {
        PostCount++;
    }

    public void DecrementPostCount()
    {
        if (PostCount > 0)
        {
            PostCount--;
        }
    }

    /// <summary>Seed/migration: gán khu cha và thứ tự hiển thị (không phát domain event).</summary>
    public void AssignHierarchy(CategoryId? parentCategoryId, int displayOrder)
    {
        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}
