using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.UpdateTag;

public sealed record UpdateTagCommand(
    int TagId,
    string Name,
    string? Description) : ICommand;
