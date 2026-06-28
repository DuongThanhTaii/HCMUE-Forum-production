using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Recruiters;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Recruiters.UpdatePermissions;

public sealed record UpdateRecruiterPermissionsCommand(
    Guid RecruiterId,
    bool CanManageJobPostings,
    bool CanReviewApplications,
    bool CanUpdateApplicationStatus,
    bool CanInviteRecruiters,
    Guid UpdatedBy) : ICommand;

internal sealed class UpdateRecruiterPermissionsCommandHandler : ICommandHandler<UpdateRecruiterPermissionsCommand>
{
    private readonly IRecruiterRepository _recruiterRepository;

    public UpdateRecruiterPermissionsCommandHandler(IRecruiterRepository recruiterRepository)
    {
        _recruiterRepository = recruiterRepository;
    }

    public async Task<Result> Handle(UpdateRecruiterPermissionsCommand request, CancellationToken cancellationToken)
    {
        // Get recruiter
        var recruiterId = RecruiterId.Create(request.RecruiterId);
        var recruiter = await _recruiterRepository.GetByIdAsync(recruiterId, cancellationToken);

        if (recruiter is null)
        {
            return Result.Failure(RecruiterErrors.NotFound);
        }

        // Create new permissions
        var permissionsResult = RecruiterPermissions.Create(
            request.CanManageJobPostings,
            request.CanReviewApplications,
            request.CanUpdateApplicationStatus,
            request.CanInviteRecruiters);

        if (permissionsResult.IsFailure)
        {
            return Result.Failure(permissionsResult.Error);
        }

        // Update permissions
        var updateResult = recruiter.UpdatePermissions(permissionsResult.Value, request.UpdatedBy);

        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        await _recruiterRepository.UpdateAsync(recruiter, cancellationToken);

        return Result.Success();
    }
}
