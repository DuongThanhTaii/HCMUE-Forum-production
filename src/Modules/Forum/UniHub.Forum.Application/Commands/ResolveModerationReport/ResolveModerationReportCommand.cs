using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.ResolveModerationReport;

public sealed record ResolveModerationReportCommand(
    int ReportId,
    Guid ReviewerId,
    string Action,
    bool IsAdmin = false,
    IReadOnlyList<Guid>? CategoryScope = null) : ICommand;
