using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniHub.Learning.Domain.Faculties;
using UniHub.Learning.Domain.Faculties.ValueObjects;

namespace UniHub.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds learning data: faculties.
/// </summary>
internal static class LearningSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Faculties.AnyAsync())
        {
            logger.LogInformation("Learning data already seeded. Skipping.");
            return;
        }

        logger.LogInformation("Seeding learning data...");

        // System user ID for seed data
        var systemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var facultyData = new[]
        {
            ("CNTT", "Khoa Công nghệ Thông tin", "Đào tạo kỹ sư CNTT, khoa học máy tính và hệ thống thông tin"),
            ("TOAN", "Khoa Toán học", "Đào tạo cử nhân Toán, Toán ứng dụng và Thống kê"),
            ("LY", "Khoa Vật lý", "Đào tạo cử nhân Vật lý và Vật lý kỹ thuật"),
            ("HOA", "Khoa Hóa học", "Đào tạo cử nhân Hóa học và Hóa ứng dụng"),
            ("SINH", "Khoa Sinh học", "Đào tạo cử nhân Sinh học và Công nghệ sinh học"),
            ("GDTH", "Khoa Giáo dục Tiểu học", "Đào tạo giáo viên tiểu học"),
            ("NNVH", "Khoa Ngôn ngữ và Văn hóa", "Đào tạo cử nhân Ngôn ngữ Anh, Ngôn ngữ Pháp"),
            ("TLGD", "Khoa Tâm lý Giáo dục", "Đào tạo cử nhân Tâm lý học và Khoa học giáo dục"),
        };

        var faculties = new List<Faculty>();
        foreach (var (code, name, desc) in facultyData)
        {
            var codeVo = FacultyCode.Create(code).Value;
            var nameVo = FacultyName.Create(name).Value;
            var descVo = FacultyDescription.Create(desc).Value;
            faculties.Add(Faculty.Create(codeVo, nameVo, descVo, systemUserId).Value);
        }

        context.Faculties.AddRange(faculties);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} faculties.", faculties.Count);
    }
}
