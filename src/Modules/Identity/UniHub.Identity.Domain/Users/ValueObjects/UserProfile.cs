using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Users.ValueObjects;

public sealed class UserProfile : ValueObject
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Avatar { get; private set; }
    public string? Bio { get; private set; }
    public string? Phone { get; private set; }
    public DateTime? DateOfBirth { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private UserProfile()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private UserProfile(string firstName, string lastName, string? avatar, string? bio, string? phone, DateTime? dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        Avatar = avatar;
        Bio = bio;
        Phone = phone;
        DateOfBirth = dateOfBirth;
    }

    public static Result<UserProfile> Create(
        string firstName,
        string lastName,
        string? avatar = null,
        string? bio = null,
        string? phone = null,
        DateTime? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure<UserProfile>(new Error("UserProfile.FirstName.Empty", "First name cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure<UserProfile>(new Error("UserProfile.LastName.Empty", "Last name cannot be empty"));
        }

        if (firstName.Trim().Length > 100)
        {
            return Result.Failure<UserProfile>(new Error("UserProfile.FirstName.TooLong", "First name cannot exceed 100 characters"));
        }

        if (lastName.Trim().Length > 100)
        {
            return Result.Failure<UserProfile>(new Error("UserProfile.LastName.TooLong", "Last name cannot exceed 100 characters"));
        }

        if (bio?.Length > 500)
        {
            return Result.Failure<UserProfile>(new Error("UserProfile.Bio.TooLong", "Bio cannot exceed 500 characters"));
        }

        if (phone is not null && !IsValidPhoneNumber(phone))
        {
            return Result.Failure<UserProfile>(new Error("UserProfile.Phone.Invalid", "Phone number format is invalid"));
        }

        if (dateOfBirth.HasValue && dateOfBirth.Value > DateTime.UtcNow.AddYears(-13))
        {
            return Result.Failure<UserProfile>(new Error("UserProfile.DateOfBirth.TooYoung", "User must be at least 13 years old"));
        }

        return Result.Success(new UserProfile(
            firstName.Trim(),
            lastName.Trim(),
            avatar?.Trim(),
            bio?.Trim(),
            phone?.Trim(),
            dateOfBirth));
    }

    private static bool IsValidPhoneNumber(string phone)
    {
        // Basic phone validation - can be enhanced based on requirements
        var cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        return cleanPhone.Length >= 8 && cleanPhone.Length <= 15 && cleanPhone.All(char.IsDigit);
    }

    public string FullName => $"{FirstName} {LastName}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Avatar ?? string.Empty;
        yield return Bio ?? string.Empty;
        yield return Phone ?? string.Empty;
        yield return DateOfBirth ?? DateTime.MinValue;
    }
}