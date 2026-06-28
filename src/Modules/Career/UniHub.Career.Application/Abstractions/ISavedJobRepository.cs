namespace UniHub.Career.Application.Abstractions;

/// <summary>
/// Represents a saved job entry (many-to-many relationship between User and JobPosting).
/// </summary>
public sealed class SavedJob
{
    public Guid UserId { get; set; }
    public Guid JobPostingId { get; set; }
    public DateTime SavedAt { get; set; }
}

/// <summary>
/// Repository interface for SavedJob operations.
/// Manages the many-to-many relationship between users and saved job postings.
/// </summary>
public interface ISavedJobRepository
{
    /// <summary>
    /// Saves a job posting to a user's saved jobs list.
    /// </summary>
    Task SaveJobAsync(Guid userId, Guid jobPostingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a job posting from a user's saved jobs list.
    /// </summary>
    Task UnsaveJobAsync(Guid userId, Guid jobPostingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all saved jobs for a user with pagination.
    /// </summary>
    Task<List<SavedJob>> GetSavedJobsByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a job posting is saved by a user.
    /// </summary>
    Task<bool> IsSavedAsync(Guid userId, Guid jobPostingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of saved jobs for a user.
    /// </summary>
    Task<int> GetSavedCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
