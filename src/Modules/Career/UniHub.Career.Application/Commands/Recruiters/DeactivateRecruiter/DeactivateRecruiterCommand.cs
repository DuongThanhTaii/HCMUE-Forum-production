using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Recruiters;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Recruiters.DeactivateRecruiter;

public sealed record DeactivateRecruiterCommand(
    Guid RecruiterId,
    Guid DeactivatedBy) : ICommand;

internal sealed class DeactivateRecruiterCommandHandler : ICommandHandler<DeactivateRecruiterCommand>
{
    private readonly IRecruiterRepository _recruiterRepository;

    public DeactivateRecruiterCommandHandler(IRecruiterRepository recruiterRepository)
    {
        _recruiterRepository = recruiterRepository;
    }

    public async Task<Result> Handle(DeactivateRecruiterCommand request, CancellationToken cancellationToken)
    {
        var recruiterId = RecruiterId.Create(request.RecruiterId);
        var recruiter = await _recruiterRepository.GetByIdAsync(recruiterId, cancellationToken);

        if (recruiter is null)
        {
            return Result.Failure(RecruiterErrors.NotFound);
        }

        var result = recruiter.Deactivate(request.DeactivatedBy);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await _recruiterRepository.UpdateAsync(recruiter, cancellationToken);

        return Result.Success();
    }
}
