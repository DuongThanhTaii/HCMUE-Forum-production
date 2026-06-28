using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Companies;

/// <summary>
/// Value object representing company social media links.
/// </summary>
public sealed class SocialLinks : ValueObject
{
    /// <summary>LinkedIn company page URL.</summary>
    public string? LinkedIn { get; private set; }

    /// <summary>Facebook page URL.</summary>
    public string? Facebook { get; private set; }

    /// <summary>Twitter/X profile URL.</summary>
    public string? Twitter { get; private set; }

    /// <summary>Instagram profile URL.</summary>
    public string? Instagram { get; private set; }

    /// <summary>YouTube channel URL.</summary>
    public string? YouTube { get; private set; }

    public const int MaxUrlLength = 500;

    /// <summary>Private constructor for EF Core.</summary>
    private SocialLinks() { }

    private SocialLinks(
        string? linkedIn,
        string? facebook,
        string? twitter,
        string? instagram,
        string? youTube)
    {
        LinkedIn = linkedIn;
        Facebook = facebook;
        Twitter = twitter;
        Instagram = instagram;
        YouTube = youTube;
    }

    /// <summary>
    /// Creates a new SocialLinks value object.
    /// </summary>
    public static Result<SocialLinks> Create(
        string? linkedIn = null,
        string? facebook = null,
        string? twitter = null,
        string? instagram = null,
        string? youTube = null)
    {
        // Trim whitespace first
        linkedIn = linkedIn?.Trim();
        facebook = facebook?.Trim();
        twitter = twitter?.Trim();
        instagram = instagram?.Trim();
        youTube = youTube?.Trim();

        // Validate URL lengths
        if (linkedIn?.Length > MaxUrlLength)
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.LinkedInTooLong", $"LinkedIn URL cannot exceed {MaxUrlLength} characters."));

        if (facebook?.Length > MaxUrlLength)
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.FacebookTooLong", $"Facebook URL cannot exceed {MaxUrlLength} characters."));

        if (twitter?.Length > MaxUrlLength)
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.TwitterTooLong", $"Twitter URL cannot exceed {MaxUrlLength} characters."));

        if (instagram?.Length > MaxUrlLength)
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.InstagramTooLong", $"Instagram URL cannot exceed {MaxUrlLength} characters."));

        if (youTube?.Length > MaxUrlLength)
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.YouTubeTooLong", $"YouTube URL cannot exceed {MaxUrlLength} characters."));

        // Basic URL validation (must start with http:// or https://)
        if (linkedIn != null && !IsValidUrl(linkedIn))
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.InvalidLinkedIn", "LinkedIn URL must start with http:// or https://"));

        if (facebook != null && !IsValidUrl(facebook))
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.InvalidFacebook", "Facebook URL must start with http:// or https://"));

        if (twitter != null && !IsValidUrl(twitter))
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.InvalidTwitter", "Twitter URL must start with http:// or https://"));

        if (instagram != null && !IsValidUrl(instagram))
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.InvalidInstagram", "Instagram URL must start with http:// or https://"));

        if (youTube != null && !IsValidUrl(youTube))
            return Result.Failure<SocialLinks>(
                new Error("SocialLinks.InvalidYouTube", "YouTube URL must start with http:// or https://"));

        return Result.Success(new SocialLinks(linkedIn, facebook, twitter, instagram, youTube));
    }

    /// <summary>
    /// Creates an empty SocialLinks with all nulls.
    /// </summary>
    public static SocialLinks Empty() => new(null, null, null, null, null);

    private static bool IsValidUrl(string url)
        => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if any social link is provided.
    /// </summary>
    public bool HasAnyLinks()
        => LinkedIn != null || Facebook != null || Twitter != null || Instagram != null || YouTube != null;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return LinkedIn;
        yield return Facebook;
        yield return Twitter;
        yield return Instagram;
        yield return YouTube;
    }
}
