using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.Forum.Domain.Tags.ValueObjects;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Tags;

public sealed class Tag : AggregateRoot<TagId>
{
    public TagName Name { get; private set; }
    public TagDescription Description { get; private set; }
    public Slug Slug { get; private set; }
    public int UsageCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor
    private Tag()
    {
        Name = null!;
        Description = null!;
        Slug = null!;
    }

    private Tag(
        TagId id,
        TagName name,
        TagDescription description,
        Slug slug)
    {
        Id = id;
        Name = name;
        Description = description;
        Slug = slug;
        UsageCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Tag> Create(
        TagId id,
        string name,
        string? description = null)
    {
        var nameResult = TagName.Create(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<Tag>(nameResult.Error);
        }

        var descriptionResult = TagDescription.Create(description);
        if (descriptionResult.IsFailure)
        {
            return Result.Failure<Tag>(descriptionResult.Error);
        }

        var slugResult = Slug.Create(name);
        if (slugResult.IsFailure)
        {
            return Result.Failure<Tag>(slugResult.Error);
        }

        var tag = new Tag(id, nameResult.Value, descriptionResult.Value, slugResult.Value);
        return Result.Success(tag);
    }

    public Result Update(string name, string? description)
    {
        var nameResult = TagName.Create(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        var descriptionResult = TagDescription.Create(description);
        if (descriptionResult.IsFailure)
        {
            return Result.Failure(descriptionResult.Error);
        }

        var slugResult = Slug.Create(name);
        if (slugResult.IsFailure)
        {
            return Result.Failure(slugResult.Error);
        }

        Name = nameResult.Value;
        Description = descriptionResult.Value;
        Slug = slugResult.Value;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void IncrementUsageCount()
    {
        UsageCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementUsageCount()
    {
        if (UsageCount > 0)
        {
            UsageCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
