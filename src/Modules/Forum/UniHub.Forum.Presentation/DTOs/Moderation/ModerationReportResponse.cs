namespace UniHub.Forum.Presentation.DTOs.Moderation;

public sealed record ModerationReportResponse
{
    public int Id { get; init; }
    public Guid ReportedItemId { get; init; }
    public int ReportedItemType { get; init; }
    public Guid ReporterId { get; init; }
    public int Reason { get; init; }
    public string? Description { get; init; }
    public int Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public Guid? ReviewedBy { get; init; }
    public string? ResolutionDecision { get; init; }
    public string? TitlePreview { get; init; }
    public string? ContentPreview { get; init; }
    public bool IsTargetDeleted { get; init; }
}
