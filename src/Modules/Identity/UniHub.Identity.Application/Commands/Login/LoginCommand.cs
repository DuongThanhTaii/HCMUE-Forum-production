using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Login;

/// <summary>
/// Command to authenticate a user
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<LoginResponse>;
