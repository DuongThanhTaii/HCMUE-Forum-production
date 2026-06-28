using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;
using System.Text.RegularExpressions;

namespace UniHub.Career.Domain.Companies;

/// <summary>
/// Value object representing company contact information.
/// </summary>
public sealed class ContactInfo : ValueObject
{
    /// <summary>Primary contact email.</summary>
    public string Email { get; private set; }

    /// <summary>Contact phone number (optional).</summary>
    public string? Phone { get; private set; }

    /// <summary>Physical address (optional).</summary>
    public string? Address { get; private set; }

    public const int MaxEmailLength = 256;
    public const int MaxPhoneLength = 20;
    public const int MaxAddressLength = 500;

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Private constructor for EF Core.</summary>
    private ContactInfo()
    {
        Email = string.Empty;
    }

    private ContactInfo(string email, string? phone, string? address)
    {
        Email = email;
        Phone = phone;
        Address = address;
    }

    /// <summary>
    /// Creates a new ContactInfo value object.
    /// </summary>
    public static Result<ContactInfo> Create(
        string email,
        string? phone = null,
        string? address = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<ContactInfo>(
                new Error("ContactInfo.EmptyEmail", "Email is required."));

        // Trim and normalize
        email = email.Trim().ToLowerInvariant();
        phone = phone?.Trim();
        address = address?.Trim();

        if (email.Length > MaxEmailLength)
            return Result.Failure<ContactInfo>(
                new Error("ContactInfo.EmailTooLong", $"Email cannot exceed {MaxEmailLength} characters."));

        if (!EmailRegex.IsMatch(email))
            return Result.Failure<ContactInfo>(
                new Error("ContactInfo.InvalidEmail", "Email format is invalid."));

        if (phone?.Length > MaxPhoneLength)
            return Result.Failure<ContactInfo>(
                new Error("ContactInfo.PhoneTooLong", $"Phone cannot exceed {MaxPhoneLength} characters."));

        if (address?.Length > MaxAddressLength)
            return Result.Failure<ContactInfo>(
                new Error("ContactInfo.AddressTooLong", $"Address cannot exceed {MaxAddressLength} characters."));

        return Result.Success(new ContactInfo(email, phone, address));
    }

    public override string ToString()
        => Phone != null ? $"{Email} | {Phone}" : Email;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
        yield return Address;
    }
}
