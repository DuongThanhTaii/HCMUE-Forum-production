using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Authorization.SetEndpointToggle;

public sealed record SetEndpointToggleCommand(
    string EndpointKey,
    bool IsEnabled,
    string UpdatedBy,
    string? Reason) : ICommand<EndpointToggleItemResponse>;
