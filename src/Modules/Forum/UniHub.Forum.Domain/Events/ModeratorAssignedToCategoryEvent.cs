using UniHub.Forum.Domain.Categories;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record ModeratorAssignedToCategoryEvent(CategoryId CategoryId, Guid ModeratorId) : IDomainEvent;
