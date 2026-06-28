using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Queries.GetReports;

public sealed class ReportDto
{
    public int Id { get; init; }
    public Guid ReportedItemId { get; init; }
    public ReportedItemType ReportedItemType { get; init; }
    public Guid ReporterId { get; init; }
    public ReportReason Reason { get; init; }
    public string? Description { get; init; }
    public ReportStatus Status { get; init; }
    public ReportResolutionDecision? ResolutionDecision { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public Guid? ReviewedBy { get; init; }
}
