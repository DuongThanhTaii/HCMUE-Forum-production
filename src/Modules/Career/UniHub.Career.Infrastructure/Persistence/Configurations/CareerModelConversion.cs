using System.Text.Json;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using UniHub.Career.Domain.Recruiters;
using CareerApplicationId = UniHub.Career.Domain.Applications.ApplicationId;

namespace UniHub.Career.Infrastructure.Persistence.Configurations;

internal static class CareerModelConversion
{
    public static CompanyId ToCompanyId(Guid value) => CompanyId.Create(value);

    public static JobPostingId ToJobPostingId(Guid value) => JobPostingId.Create(value);

    public static CareerApplicationId ToApplicationId(Guid value) => CareerApplicationId.Create(value);

    public static RecruiterId ToRecruiterId(Guid value) => RecruiterId.Create(value);

    public static string ToContactInfoDb(ContactInfo value)
    {
        var dto = new ContactInfoDto(value.Email, value.Phone, value.Address);
        return JsonSerializer.Serialize(dto);
    }

    public static ContactInfo ToContactInfoDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<ContactInfoDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize contact info");

        return ContactInfo.Create(dto.Email, dto.Phone, dto.Address).Value;
    }

    public static string ToSocialLinksDb(SocialLinks value)
    {
        var dto = new SocialLinksDto(
            value.LinkedIn,
            value.Facebook,
            value.Twitter,
            value.Instagram,
            value.YouTube);

        return JsonSerializer.Serialize(dto);
    }

    public static SocialLinks ToSocialLinksDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<SocialLinksDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize social links");

        return SocialLinks.Create(
            dto.LinkedIn,
            dto.Facebook,
            dto.Twitter,
            dto.Instagram,
            dto.YouTube).Value;
    }

    public static string ToWorkLocationDb(WorkLocation value)
    {
        var dto = new WorkLocationDto(value.City, value.District, value.Address, value.IsRemote);
        return JsonSerializer.Serialize(dto);
    }

    public static WorkLocation ToWorkLocationDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<WorkLocationDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize work location");

        return WorkLocation.Create(dto.City, dto.District, dto.Address, dto.IsRemote).Value;
    }

    public static string? ToSalaryRangeDb(SalaryRange? value)
    {
        if (value is null)
        {
            return null;
        }

        var dto = new SalaryRangeDto(value.MinAmount, value.MaxAmount, value.Currency, value.Period);
        return JsonSerializer.Serialize(dto);
    }

    public static SalaryRange? ToSalaryRangeDomain(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var dto = JsonSerializer.Deserialize<SalaryRangeDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize salary range");

        return SalaryRange.Create(dto.MinAmount, dto.MaxAmount, dto.Currency, dto.Period).Value;
    }

    public static string ToResumeDb(Resume value)
    {
        var dto = new ResumeDto(value.FileName, value.FileUrl, value.FileSizeBytes, value.ContentType);
        return JsonSerializer.Serialize(dto);
    }

    public static Resume ToResumeDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<ResumeDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize resume");

        return Resume.Create(dto.FileName, dto.FileUrl, dto.FileSizeBytes, dto.ContentType).Value;
    }

    public static string? ToCoverLetterDb(CoverLetter? value)
    {
        if (value is null)
        {
            return null;
        }

        return value.Content;
    }

    public static CoverLetter? ToCoverLetterDomain(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return CoverLetter.Create(raw).Value;
    }

    public static string ToJobRequirementListDb(List<JobRequirement> values)
    {
        var dto = values.Select(v => new JobRequirementDto(v.Skill, v.IsRequired)).ToList();
        return JsonSerializer.Serialize(dto);
    }

    public static List<JobRequirement> ToJobRequirementListDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<List<JobRequirementDto>>(raw) ?? new List<JobRequirementDto>();
        return dto.Select(v => JobRequirement.Create(v.Skill, v.IsRequired).Value).ToList();
    }

    public static string ToStringListDb(List<string> values) => JsonSerializer.Serialize(values);

    public static List<string> ToStringListDomain(string raw)
        => JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();

    public static string ToRecruiterPermissionsDb(RecruiterPermissions value)
    {
        var dto = new RecruiterPermissionsDto(
            value.CanManageJobPostings,
            value.CanReviewApplications,
            value.CanUpdateApplicationStatus,
            value.CanInviteRecruiters);

        return JsonSerializer.Serialize(dto);
    }

    public static RecruiterPermissions ToRecruiterPermissionsDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<RecruiterPermissionsDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize recruiter permissions");

        return RecruiterPermissions.Create(
            dto.CanManageJobPostings,
            dto.CanReviewApplications,
            dto.CanUpdateApplicationStatus,
            dto.CanInviteRecruiters).Value;
    }

    private sealed record ContactInfoDto(string Email, string? Phone, string? Address);

    private sealed record SocialLinksDto(
        string? LinkedIn,
        string? Facebook,
        string? Twitter,
        string? Instagram,
        string? YouTube);

    private sealed record WorkLocationDto(string City, string? District, string? Address, bool IsRemote);

    private sealed record SalaryRangeDto(decimal MinAmount, decimal MaxAmount, string Currency, string Period);

    private sealed record ResumeDto(string FileName, string FileUrl, long FileSizeBytes, string ContentType);

    private sealed record JobRequirementDto(string Skill, bool IsRequired);

    private sealed record RecruiterPermissionsDto(
        bool CanManageJobPostings,
        bool CanReviewApplications,
        bool CanUpdateApplicationStatus,
        bool CanInviteRecruiters);
}
