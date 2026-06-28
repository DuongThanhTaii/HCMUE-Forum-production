using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Queries.GetPostById;

/// <summary>
/// Query to get a post by ID
/// </summary>
public sealed record GetPostByIdQuery(Guid PostId, Guid? CurrentUserId = null) : IQuery<PostDetailResult?>;
