namespace UniHub.Forum.Presentation.DTOs.Reports;

/// <summary>
/// Request to report a post or comment
/// </summary>
public sealed record ReportRequest
{
    /// <summary>
    /// Report reason (1=Spam, 2=Harassment, 3=InappropriateContent, 4=Misinformation, 5=OffTopic, 6=CopyrightViolation, 99=Other)
    /// </summary>
    public int Reason { get; init; }

    /// <summary>
    /// Optional description providing additional context
    /// </summary>
    public string? Description { get; init; }
}
