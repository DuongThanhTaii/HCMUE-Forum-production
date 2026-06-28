using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.Recruiters.AddRecruiter;

public sealed record AddRecruiterCommand(
    Guid UserId,
    Guid CompanyId,
    bool CanManageJobPostings = true,
    bool CanReviewApplications = true,
    bool CanUpdateApplicationStatus = true,
    bool CanInviteRecruiters = false,
    Guid AddedBy = default) : ICommand<RecruiterResponse>;

public sealed record RecruiterResponse(
    Guid RecruiterId,
    Guid UserId,
    Guid CompanyId,
    RecruiterPermissionsDto Permissions,
    string Status,
    Guid AddedBy,
    DateTime AddedAt);

public sealed record RecruiterPermissionsDto(
    bool CanManageJobPostings,
    bool CanReviewApplications,
    bool CanUpdateApplicationStatus,
    bool CanInviteRecruiters);
