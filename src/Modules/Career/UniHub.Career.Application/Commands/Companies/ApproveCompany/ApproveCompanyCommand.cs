using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.Companies.ApproveCompany;

public sealed record ApproveCompanyCommand(
    Guid CompanyId,
    Guid ApprovedBy) : ICommand;
