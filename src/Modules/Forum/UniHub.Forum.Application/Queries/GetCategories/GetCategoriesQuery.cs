using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Queries.GetCategories;

public sealed record CategoryListItem(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    Guid? ParentCategoryId,
    int PostCount,
    int DisplayOrder);

public sealed record GetCategoriesResult(IReadOnlyList<CategoryListItem> Categories);

public sealed record GetCategoriesQuery : IQuery<GetCategoriesResult>;
