using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.DeleteTag;

public sealed record DeleteTagCommand(int TagId) : ICommand;
