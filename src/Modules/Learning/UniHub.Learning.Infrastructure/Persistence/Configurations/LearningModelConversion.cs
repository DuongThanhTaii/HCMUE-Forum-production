using System.Text.Json;
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Courses.ValueObjects;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.ValueObjects;
using UniHub.Learning.Domain.Faculties;
using UniHub.Learning.Domain.Faculties.ValueObjects;

namespace UniHub.Learning.Infrastructure.Persistence.Configurations;

internal static class LearningModelConversion
{
    public static CourseId ToCourseId(Guid value) => CourseId.Create(value);

    public static FacultyId ToFacultyId(Guid value) => FacultyId.Create(value);

    public static DocumentId ToDocumentId(Guid value) => DocumentId.Create(value);

    public static CourseCode ToCourseCode(string raw) => CourseCode.Create(raw).Value;

    public static CourseName ToCourseName(string raw) => CourseName.Create(raw).Value;

    public static CourseDescription ToCourseDescription(string raw) => CourseDescription.Create(raw).Value;

    public static Semester ToSemester(string raw) => Semester.Create(raw).Value;

    public static FacultyCode ToFacultyCode(string raw) => FacultyCode.Create(raw).Value;

    public static FacultyName ToFacultyName(string raw) => FacultyName.Create(raw).Value;

    public static FacultyDescription ToFacultyDescription(string raw) => FacultyDescription.Create(raw).Value;

    public static DocumentTitle ToDocumentTitle(string raw) => DocumentTitle.Create(raw).Value;

    public static DocumentDescription ToDocumentDescription(string raw) => DocumentDescription.Create(raw).Value;

    public static string ToDocumentFileDb(DocumentFile file)
    {
        var dto = new DocumentFileDto(file.FileName, file.FilePath, file.FileSize, file.ContentType);
        return JsonSerializer.Serialize(dto);
    }

    public static DocumentFile ToDocumentFileDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<DocumentFileDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize document file");

        return DocumentFile.Create(dto.FileName, dto.FilePath, dto.FileSize, dto.ContentType).Value;
    }

    public static string ToGuidListDb(List<Guid> values) => JsonSerializer.Serialize(values);

    public static List<Guid> ToGuidListDomain(string raw)
        => JsonSerializer.Deserialize<List<Guid>>(raw) ?? new List<Guid>();

    private sealed record DocumentFileDto(
        string FileName,
        string FilePath,
        long FileSize,
        string ContentType);
}
