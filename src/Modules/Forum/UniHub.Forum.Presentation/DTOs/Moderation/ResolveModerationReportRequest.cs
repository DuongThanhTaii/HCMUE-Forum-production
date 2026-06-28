namespace UniHub.Forum.Presentation.DTOs.Moderation;

public sealed record ResolveModerationReportRequest
{
    public string Action { get; init; } = string.Empty;
}
