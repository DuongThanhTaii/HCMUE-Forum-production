using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// Value object representing a required skill for a job posting.
/// </summary>
public sealed class JobRequirement : ValueObject
{
    /// <summary>Skill or requirement name (e.g., "C#", "React", "English B2").</summary>
    public string Skill { get; private set; }

    /// <summary>Whether this skill is mandatory or nice-to-have.</summary>
    public bool IsRequired { get; private set; }

    public const int MaxSkillLength = 100;

    /// <summary>Private constructor for EF Core.</summary>
    private JobRequirement()
    {
        Skill = string.Empty;
    }

    private JobRequirement(string skill, bool isRequired)
    {
        Skill = skill;
        IsRequired = isRequired;
    }

    /// <summary>
    /// Creates a new JobRequirement value object.
    /// </summary>
    public static Result<JobRequirement> Create(string skill, bool isRequired = true)
    {
        if (string.IsNullOrWhiteSpace(skill))
            return Result.Failure<JobRequirement>(
                new Error("JobRequirement.EmptySkill", "Skill name is required."));

        if (skill.Length > MaxSkillLength)
            return Result.Failure<JobRequirement>(
                new Error("JobRequirement.SkillTooLong", $"Skill name cannot exceed {MaxSkillLength} characters."));

        return Result.Success(new JobRequirement(skill.Trim(), isRequired));
    }

    public override string ToString()
        => IsRequired ? $"{Skill} (required)" : $"{Skill} (preferred)";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Skill;
        yield return IsRequired;
    }
}
