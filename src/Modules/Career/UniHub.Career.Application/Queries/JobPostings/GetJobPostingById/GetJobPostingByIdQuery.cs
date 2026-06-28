using UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Queries.JobPostings.GetJobPostingById;

/// <summary>
/// Query to get a job posting by ID.
/// </summary>
public sealed record GetJobPostingByIdQuery(
    Guid JobPostingId) : IQuery<JobPostingResponse>;
