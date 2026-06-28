using UniHub.Forum.Domain.Categories;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record ModeratorRemovedFromCategoryEvent(CategoryId CategoryId, Guid ModeratorId) : IDomainEvent;
