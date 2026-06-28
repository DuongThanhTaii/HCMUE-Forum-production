using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Users.ValueObjects;

public sealed class OfficialBadge : ValueObject
{
    public BadgeType Type { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime VerifiedAt { get; private set; }
    public string VerifiedBy { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private OfficialBadge()
    {
        Name = string.Empty;
        VerifiedBy = string.Empty;
    }

    private OfficialBadge(BadgeType type, string name, string? description, DateTime verifiedAt, string verifiedBy)
    {
        Type = type;
        Name = name;
        Description = description;
        VerifiedAt = verifiedAt;
        VerifiedBy = verifiedBy;
    }

    public static Result<OfficialBadge> Create(
        BadgeType type,
        string name,
        string verifiedBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<OfficialBadge>(new Error("OfficialBadge.Name.Empty", "Badge name cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(verifiedBy))
        {
            return Result.Failure<OfficialBadge>(new Error("OfficialBadge.VerifiedBy.Empty", "Verified by cannot be empty"));
        }

        if (name.Trim().Length > 100)
        {
            return Result.Failure<OfficialBadge>(new Error("OfficialBadge.Name.TooLong", "Badge name cannot exceed 100 characters"));
        }

        if (description?.Length > 200)
        {
            return Result.Failure<OfficialBadge>(new Error("OfficialBadge.Description.TooLong", "Badge description cannot exceed 200 characters"));
        }

        return Result.Success(new OfficialBadge(
            type,
            name.Trim(),
            description?.Trim(),
            DateTime.UtcNow,
            verifiedBy.Trim()));
    }

    public string DisplayText => Type switch
    {
        BadgeType.Department => $"🔵 {Name}",
        BadgeType.Club => $"🟢 {Name}",
        BadgeType.BoardOfDirectors => $"🟡 {Name}",
        BadgeType.Faculty => $"🟣 {Name}",
        BadgeType.Company => $"🟠 {Name}",
        _ => Name
    };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Name;
        yield return Description ?? string.Empty;
        yield return VerifiedBy;
    }
}

public enum BadgeType
{
    Department = 1,        // 🔵 Blue - Phòng ban chính thức
    Club = 2,             // 🟢 Green - CLB/Đoàn thể
    BoardOfDirectors = 3, // 🟡 Gold - Ban Giám hiệu
    Faculty = 4,          // 🟣 Purple - Giảng viên
    Company = 5           // 🟠 Orange - Doanh nghiệp đối tác
}