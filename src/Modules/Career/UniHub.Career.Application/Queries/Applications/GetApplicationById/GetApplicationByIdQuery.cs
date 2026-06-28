using UniHub.Career.Application.Commands.Applications.SubmitApplication;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Queries.Applications.GetApplicationById;

/// <summary>
/// Query to retrieve a single application by ID.
/// </summary>
public sealed record GetApplicationByIdQuery(Guid ApplicationId) : IQuery<ApplicationResponse>;
