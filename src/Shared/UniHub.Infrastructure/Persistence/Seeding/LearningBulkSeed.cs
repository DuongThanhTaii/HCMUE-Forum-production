using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Courses.ValueObjects;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.ValueObjects;
using UniHub.Learning.Domain.Faculties;

namespace UniHub.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds many courses and approved documents for local FE/API testing (pagination, filters).
/// Controlled via configuration <c>Seeding:Learning:Bulk:*</c>.
/// </summary>
internal static class LearningBulkSeed
{
    private static readonly Guid DefaultActorId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration configuration, ILogger logger)
    {
        var enabled = configuration.GetValue("Seeding:Learning:Bulk:Enabled", false);
        if (!enabled)
        {
            return;
        }

        var onlyIfEmpty = configuration.GetValue("Seeding:Learning:Bulk:OnlyIfEmpty", true);
        if (onlyIfEmpty && await context.Documents.AnyAsync())
        {
            logger.LogInformation(
                "Learning bulk seed skipped (documents already exist). Set Seeding:Learning:Bulk:OnlyIfEmpty=false to force.");
            return;
        }

        var coursesPerFaculty = Math.Clamp(configuration.GetValue("Seeding:Learning:Bulk:CoursesPerFaculty", 10), 1, 50);
        var documentsPerCourse = Math.Clamp(configuration.GetValue("Seeding:Learning:Bulk:DocumentsPerCourse", 15), 1, 100);
        var maxDocuments = configuration.GetValue("Seeding:Learning:Bulk:MaxDocuments", 5000);
        var reviewerId = configuration.GetValue("Seeding:Learning:Bulk:ReviewerUserId", DefaultActorId);
        var uploaderId = configuration.GetValue("Seeding:Learning:Bulk:UploaderUserId", DefaultActorId);
        var batchSize = Math.Clamp(configuration.GetValue("Seeding:Learning:Bulk:SaveBatchSize", 150), 50, 500);

        var faculties = await context.Faculties.AsTracking().ToListAsync();
        if (faculties.Count == 0)
        {
            logger.LogWarning("Learning bulk seed skipped: no faculties (run LearningSeed first).");
            return;
        }

        logger.LogInformation(
            "Learning bulk seed: faculties={Fc}, courses/faculty={Cpf}, documents/course={Dpc}, reviewer={Rev}",
            faculties.Count,
            coursesPerFaculty,
            documentsPerCourse,
            reviewerId);

        var newCourses = new List<Course>();
        foreach (var faculty in faculties)
        {
            var codePrefix = faculty.Code.Value.Length <= 12 ? faculty.Code.Value : faculty.Code.Value[..12];
            for (var i = 1; i <= coursesPerFaculty; i++)
            {
                var codeStr = $"{codePrefix}-{i:D2}";
                var code = CourseCode.Create(codeStr);
                if (code.IsFailure)
                {
                    throw new InvalidOperationException($"Invalid seed course code '{codeStr}': {code.Error.Message}");
                }

                var name = CourseName.Create($"{faculty.Name.Value} — Môn mẫu {i}");
                if (name.IsFailure)
                {
                    throw new InvalidOperationException(name.Error.Message);
                }

                var desc = CourseDescription.Create($"Khóa học seed cho khoa {faculty.Code.Value}, học phần {i}.");
                if (desc.IsFailure)
                {
                    throw new InvalidOperationException(desc.Error.Message);
                }

                var semester = Semester.CreateFromYearAndTerm(2025, (i % 3) + 1);
                if (semester.IsFailure)
                {
                    throw new InvalidOperationException(semester.Error.Message);
                }

                var credits = 2 + (i % 7);
                var courseResult = Course.Create(
                    code.Value,
                    name.Value,
                    desc.Value,
                    semester.Value,
                    credits,
                    uploaderId,
                    faculty.Id.Value);

                if (courseResult.IsFailure)
                {
                    throw new InvalidOperationException(courseResult.Error.Message);
                }

                var course = courseResult.Value;
                faculty.IncrementCourseCount();
                newCourses.Add(course);
            }
        }

        context.Courses.AddRange(newCourses);
        await context.SaveChangesAsync();

        var documentsBuffer = new List<Document>();
        var totalWritten = 0;
        var docBudget = maxDocuments;
        foreach (var course in newCourses)
        {
            for (var d = 1; d <= documentsPerCourse && docBudget > 0; d++)
            {
                docBudget--;
                var title = DocumentTitle.Create(
                    $"Seed tài liệu {course.Code.Value} — phần {d} (đủ độ dài tiêu đề)");
                if (title.IsFailure)
                {
                    throw new InvalidOperationException(title.Error.Message);
                }

                var description = DocumentDescription.Create(
                    $"Nội dung mô phỏng cho kiểm thử danh sách và phân trang. Course={course.Id.Value}, index={d}.");
                if (description.IsFailure)
                {
                    throw new InvalidOperationException(description.Error.Message);
                }

                var file = DocumentFile.Create(
                    $"seed-{course.Code.Value}-{d}.pdf",
                    $"/seed/learning/{course.Id.Value}/{d}.pdf",
                    2048 + d,
                    "application/pdf");
                if (file.IsFailure)
                {
                    throw new InvalidOperationException(file.Error.Message);
                }

                var docType = (DocumentType)(((d - 1) % 6) + 1);
                var docResult = Document.Create(
                    title.Value,
                    description.Value,
                    file.Value,
                    docType,
                    uploaderId,
                    course.Id.Value);

                if (docResult.IsFailure)
                {
                    throw new InvalidOperationException(docResult.Error.Message);
                }

                var document = docResult.Value;
                var submit = document.SubmitForApproval();
                if (submit.IsFailure)
                {
                    throw new InvalidOperationException(submit.Error.Message);
                }

                var approve = document.Approve(reviewerId, "Bulk seed approval");
                if (approve.IsFailure)
                {
                    throw new InvalidOperationException(approve.Error.Message);
                }

                course.IncrementDocumentCount();
                documentsBuffer.Add(document);

                if (documentsBuffer.Count >= batchSize)
                {
                    context.Documents.AddRange(documentsBuffer);
                    await context.SaveChangesAsync();
                    totalWritten += documentsBuffer.Count;
                    documentsBuffer.Clear();
                }
            }
        }

        if (documentsBuffer.Count > 0)
        {
            context.Documents.AddRange(documentsBuffer);
            await context.SaveChangesAsync();
            totalWritten += documentsBuffer.Count;
        }

        logger.LogInformation(
            "Learning bulk seed completed: courses={Courses}, documents={Docs}",
            newCourses.Count,
            totalWritten);
    }
}
