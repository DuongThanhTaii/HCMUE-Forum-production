using UniHub.SharedKernel.CQRS;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Commands.ReportPost;

public sealed record ReportPostCommand(
    Guid PostId,
    Guid ReporterId,
    ReportReason Reason,
    string? Description) : ICommand<int>;
