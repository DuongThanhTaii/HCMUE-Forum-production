using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Recruiters.AddRecruiter;

internal sealed class AddRecruiterCommandHandler : ICommandHandler<AddRecruiterCommand, RecruiterResponse>
{
    private readonly IRecruiterRepository _recruiterRepository;
    private readonly ICompanyRepository _companyRepository;

    public AddRecruiterCommandHandler(
        IRecruiterRepository recruiterRepository,
        ICompanyRepository companyRepository)
    {
        _recruiterRepository = recruiterRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<RecruiterResponse>> Handle(AddRecruiterCommand request, CancellationToken cancellationToken)
    {
        // Verify company exists
        var companyId = CompanyId.Create(request.CompanyId);
        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            return Result.Failure<RecruiterResponse>(new Error(
                "Company.NotFound",
                $"Company with ID {request.CompanyId} was not found"));
        }

        // Check if user is already a recruiter for this company
        var exists = await _recruiterRepository.ExistsAsync(request.UserId, companyId, cancellationToken);
        if (exists)
        {
            return Result.Failure<RecruiterResponse>(RecruiterErrors.AlreadyExists);
        }

        // Create permissions
        var permissionsResult = RecruiterPermissions.Create(
            request.CanManageJobPostings,
            request.CanReviewApplications,
            request.CanUpdateApplicationStatus,
            request.CanInviteRecruiters);

        if (permissionsResult.IsFailure)
        {
            return Result.Failure<RecruiterResponse>(permissionsResult.Error);
        }

        // Add recruiter
        var recruiterResult = Recruiter.Add(
            request.UserId,
            companyId,
            permissionsResult.Value,
            request.AddedBy);

        if (recruiterResult.IsFailure)
        {
            return Result.Failure<RecruiterResponse>(recruiterResult.Error);
        }

        var recruiter = recruiterResult.Value;

        await _recruiterRepository.AddAsync(recruiter, cancellationToken);

        // Map to response
        var response = new RecruiterResponse(
            recruiter.Id.Value,
            recruiter.UserId,
            recruiter.CompanyId.Value,
            new RecruiterPermissionsDto(
                recruiter.Permissions.CanManageJobPostings,
                recruiter.Permissions.CanReviewApplications,
                recruiter.Permissions.CanUpdateApplicationStatus,
                recruiter.Permissions.CanInviteRecruiters),
            recruiter.Status.ToString(),
            recruiter.AddedBy,
            recruiter.AddedAt);

        return Result.Success(response);
    }
}
