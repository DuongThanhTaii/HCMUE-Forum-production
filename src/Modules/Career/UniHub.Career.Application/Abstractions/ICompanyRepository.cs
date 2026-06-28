using UniHub.Career.Domain.Companies;

namespace UniHub.Career.Application.Abstractions;

/// <summary>
/// Repository interface for Company aggregate.
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Adds a new company to the repository.
    /// </summary>
    Task AddAsync(Company company, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a company by its ID.
    /// </summary>
    Task<Company?> GetByIdAsync(CompanyId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a company by its name.
    /// </summary>
    Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a company name is unique.
    /// </summary>
    Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing company.
    /// </summary>
    Task UpdateAsync(Company company, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all companies with pagination.
    /// </summary>
    Task<List<Company>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets companies by status.
    /// </summary>
    Task<List<Company>> GetByStatusAsync(CompanyStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets companies created by a specific user.
    /// </summary>
    Task<List<Company>> GetByRegisteredByAsync(Guid registeredBy, CancellationToken cancellationToken = default);
}
