using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Recruiters;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Recruiters.ReactivateRecruiter;

public sealed record ReactivateRecruiterCommand(
    Guid RecruiterId,
    Guid ReactivatedBy) : ICommand;

internal sealed class ReactivateRecruiterCommandHandler : ICommandHandler<ReactivateRecruiterCommand>
{
    private readonly IRecruiterRepository _recruiterRepository;

    public ReactivateRecruiterCommandHandler(IRecruiterRepository recruiterRepository)
    {
        _recruiterRepository = recruiterRepository;
    }

    public async Task<Result> Handle(ReactivateRecruiterCommand request, CancellationToken cancellationToken)
    {
        var recruiterId = RecruiterId.Create(request.RecruiterId);
        var recruiter = await _recruiterRepository.GetByIdAsync(recruiterId, cancellationToken);

        if (recruiter is null)
        {
            return Result.Failure(RecruiterErrors.NotFound);
        }

        var result = recruiter.Reactivate(request.ReactivatedBy);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await _recruiterRepository.UpdateAsync(recruiter, cancellationToken);

        return Result.Success();
    }
}
