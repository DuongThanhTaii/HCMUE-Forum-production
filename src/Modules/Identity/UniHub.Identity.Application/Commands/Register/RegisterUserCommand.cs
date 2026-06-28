using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Register;

/// <summary>
/// Command to register a new user
/// </summary>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FullName,
    string? Bio = null,
    string? AvatarUrl = null) : ICommand<Guid>;
