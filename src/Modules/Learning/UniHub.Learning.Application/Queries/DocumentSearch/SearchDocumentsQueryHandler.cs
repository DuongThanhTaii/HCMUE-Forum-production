using UniHub.Learning.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.DocumentSearch;

public sealed class SearchDocumentsQueryHandler : IQueryHandler<SearchDocumentsQuery, SearchDocumentsResult>
{
    private readonly IDocumentRepository _documentRepository;

    public SearchDocumentsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<SearchDocumentsResult>> Handle(
        SearchDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        // Call repository search method
        var (documents, totalCount) = await _documentRepository.SearchAsync(
            request.SearchTerm,
            request.CourseId,
            request.FacultyId,
            request.DocumentType,
            request.Status,
            request.SortBy,
            request.SortDescending,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        // Map to DTOs
        var documentDtos = documents.Select(d => new DocumentSearchDto(
            d.Id.Value,
            d.Title.Value,
            d.Description.Value,
            d.Type.ToString(),
            d.Status.ToString(),
            d.File.FileName,
            d.File.FileSize,
            d.File.ContentType,
            (decimal)d.AverageRating,
            d.RatingCount,
            d.ViewCount,
            d.DownloadCount,
            d.UploaderId,
            d.CourseId,
            d.CreatedAt,
            d.UpdatedAt ?? d.CreatedAt)).ToList();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var result = new SearchDocumentsResult(
            documentDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Success(result);
    }
}
