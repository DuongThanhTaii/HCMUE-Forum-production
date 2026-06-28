using MediatR;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Events;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Commands.ReportComment;

public sealed class ReportCommentCommandHandler : ICommandHandler<ReportCommentCommand, int>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IPublisher _publisher;

    public ReportCommentCommandHandler(
        IReportRepository reportRepository,
        ICommentRepository commentRepository,
        IPublisher publisher)
    {
        _reportRepository = reportRepository;
        _commentRepository = commentRepository;
        _publisher = publisher;
    }

    public async Task<Result<int>> Handle(
        ReportCommentCommand request,
        CancellationToken cancellationToken)
    {
        // Check if comment exists
        var comment = await _commentRepository.GetByIdAsync(
            new CommentId(request.CommentId),
            cancellationToken);

        if (comment is null)
        {
            return Result.Failure<int>(ReportErrors.CommentNotFound);
        }

        // Check if user has already reported this comment
        var existingReport = await _reportRepository.GetByReporterAndItemAsync(
            request.ReporterId,
            request.CommentId,
            ReportedItemType.Comment,
            cancellationToken);

        if (existingReport is not null)
        {
            return Result.Failure<int>(ReportErrors.DuplicateReport);
        }

        // Create report
        var reportId = new ReportId(0); // Will be assigned by database
        var reportResult = Report.Create(
            reportId,
            request.CommentId,
            ReportedItemType.Comment,
            request.ReporterId,
            request.Reason,
            request.Description);

        if (reportResult.IsFailure)
        {
            return Result.Failure<int>(reportResult.Error);
        }

        await _reportRepository.AddAsync(reportResult.Value, cancellationToken);

        await _publisher.Publish(
            new ReportSubmittedEvent(
                reportResult.Value.Id.Value,
                request.CommentId,
                ReportedItemType.Comment,
                request.ReporterId,
                request.Reason),
            cancellationToken);

        return Result.Success(reportResult.Value.Id.Value);
    }
}
