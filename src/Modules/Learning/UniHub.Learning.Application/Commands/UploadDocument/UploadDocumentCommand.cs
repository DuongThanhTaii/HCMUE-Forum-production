using UniHub.Learning.Domain.Documents;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.UploadDocument;

/// <summary>
/// Command to upload a new document
/// </summary>
/// <param name="Title">Title of the document</param>
/// <param name="Description">Description of the document</param>
/// <param name="FileName">Original filename</param>
/// <param name="FileContent">File content as byte array</param>
/// <param name="ContentType">MIME type of the file</param>
/// <param name="FileSize">Size of file in bytes</param>
/// <param name="DocumentType">Type of document (Slide, Exam, etc.)</param>
/// <param name="UploaderId">User who uploads the document</param>
/// <param name="CourseId">Optional course ID</param>
public sealed record UploadDocumentCommand(
    string Title,
    string Description,
    string FileName,
    byte[] FileContent,
    string ContentType,
    long FileSize,
    DocumentType DocumentType,
    Guid UploaderId,
    Guid? CourseId = null) : ICommand<Guid>;
