using UniHub.SharedKernel.CQRS;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Commands.ReportComment;

public sealed record ReportCommentCommand(
    Guid CommentId,
    Guid ReporterId,
    ReportReason Reason,
    string? Description) : ICommand<int>;
