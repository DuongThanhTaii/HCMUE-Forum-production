using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Users.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private Email() { Value = string.Empty; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(new Error("Email.Empty", "Email cannot be empty"));
        }

        var trimmedEmail = email.Trim().ToLowerInvariant();

        if (!IsValidEmailFormat(trimmedEmail))
        {
            return Result.Failure<Email>(new Error("Email.Invalid", "Email format is invalid"));
        }

        if (trimmedEmail.Length > 256)
        {
            return Result.Failure<Email>(new Error("Email.TooLong", "Email cannot exceed 256 characters"));
        }

        return Result.Success(new Email(trimmedEmail));
    }

    private static bool IsValidEmailFormat(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}