using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters;

namespace UniHub.Career.Application.Abstractions;

public interface IRecruiterRepository
{
    Task AddAsync(Recruiter recruiter, CancellationToken cancellationToken = default);
    Task UpdateAsync(Recruiter recruiter, CancellationToken cancellationToken = default);
    Task<Recruiter?> GetByIdAsync(RecruiterId id, CancellationToken cancellationToken = default);
    Task<Recruiter?> GetByUserAndCompanyAsync(Guid userId, CompanyId companyId, CancellationToken cancellationToken = default);
    Task<List<Recruiter>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Recruiter>> GetByCompanyAsync(CompanyId companyId, CancellationToken cancellationToken = default);
    Task<List<Recruiter>> GetActiveByCompanyAsync(CompanyId companyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CompanyId companyId, CancellationToken cancellationToken = default);
    Task<int> GetActiveCountAsync(CompanyId companyId, CancellationToken cancellationToken = default);
}
