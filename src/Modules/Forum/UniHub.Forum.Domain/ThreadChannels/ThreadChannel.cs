using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.ThreadChannels;

public sealed class ThreadChannel : AggregateRoot<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    public bool AllowPinnedComments { get; private set; }
    public bool AllowAcceptedAnswers { get; private set; }
    public bool AllowModeratorActions { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ThreadChannel()
    {
    }

    private ThreadChannel(
        Guid id,
        string code,
        string name,
        string? description,
        int displayOrder,
        bool isActive,
        bool allowPinnedComments,
        bool allowAcceptedAnswers,
        bool allowModeratorActions)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = name.Trim();
        Description = NormalizeDescription(description);
        DisplayOrder = displayOrder;
        IsActive = isActive;
        AllowPinnedComments = allowPinnedComments;
        AllowAcceptedAnswers = allowAcceptedAnswers;
        AllowModeratorActions = allowModeratorActions;
        CreatedAt = DateTime.UtcNow;
    }

    public static ThreadChannel Create(
        string code,
        string name,
        string? description,
        int displayOrder,
        bool isActive,
        bool allowPinnedComments,
        bool allowAcceptedAnswers,
        bool allowModeratorActions)
    {
        return new ThreadChannel(
            Guid.NewGuid(),
            code,
            name,
            description,
            displayOrder,
            isActive,
            allowPinnedComments,
            allowAcceptedAnswers,
            allowModeratorActions);
    }

    public void Update(
        string code,
        string name,
        string? description,
        int displayOrder,
        bool isActive,
        bool allowPinnedComments,
        bool allowAcceptedAnswers,
        bool allowModeratorActions)
    {
        Code = NormalizeCode(code);
        Name = name.Trim();
        Description = NormalizeDescription(description);
        DisplayOrder = displayOrder;
        IsActive = isActive;
        AllowPinnedComments = allowPinnedComments;
        AllowAcceptedAnswers = allowAcceptedAnswers;
        AllowModeratorActions = allowModeratorActions;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToLowerInvariant();
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        return description.Trim();
    }
}
