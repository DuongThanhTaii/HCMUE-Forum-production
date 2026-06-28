using UniHub.Forum.Domain.Categories;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record CategoryDeactivatedEvent(CategoryId CategoryId) : IDomainEvent;
