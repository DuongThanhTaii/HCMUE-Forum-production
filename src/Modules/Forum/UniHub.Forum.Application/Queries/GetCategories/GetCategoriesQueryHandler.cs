using UniHub.Forum.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, GetCategoriesResult>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<GetCategoriesResult>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var all = await _categoryRepository.GetAllAsync(cancellationToken);
        var items = all
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name.Value)
            .Select(c => new CategoryListItem(
                c.Id.Value,
                c.Name.Value,
                c.Description.Value,
                c.Slug.Value,
                c.ParentCategoryId?.Value,
                c.PostCount,
                c.DisplayOrder))
            .ToList();

        return Result.Success(new GetCategoriesResult(items));
    }
}
