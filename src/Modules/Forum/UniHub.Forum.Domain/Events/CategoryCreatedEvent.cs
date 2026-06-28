using UniHub.Forum.Domain.Categories;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record CategoryCreatedEvent(CategoryId CategoryId, string Name) : IDomainEvent;
