using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.CreateTag;

public sealed record CreateTagCommand(
    string Name,
    string? Description) : ICommand<int>;
