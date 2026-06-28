using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using UniHub.Career.Domain.Recruiters;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds baseline career data: companies, recruiters and published job postings.
/// </summary>
internal static class CareerSeed
{
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding career data...");
        var companies = await EnsureCompaniesAsync(context, logger, SystemUserId);

        var companyRecruiterMap = await BuildCompanyRecruiterMapAsync(context, companies);
        await EnsureRecruitersAsync(context, logger, companyRecruiterMap);
        await EnsureJobPostingsAsync(context, logger, companies, companyRecruiterMap);
    }

    private static async Task<List<Company>> EnsureCompaniesAsync(
        ApplicationDbContext context,
        ILogger logger,
        Guid recruiterUserId)
    {
        var seeded = new List<Company>();
        var companySpecs = new[]
        {
            new
            {
                Name = "Bosch Global Software Technologies Vietnam",
                Description = "Bosch software center in Vietnam focusing on embedded systems, automotive and enterprise platforms.",
                Industry = Industry.Technology,
                Size = CompanySize.Enterprise,
                Website = "https://www.bosch.com.vn",
                LogoUrl = "https://logo.clearbit.com/bosch.com",
                FoundedYear = 1886,
                Email = "careers.vn@bosch.com",
                Phone = "+84 28 3812 3456",
                Address = "Tan Binh, Ho Chi Minh City",
                LinkedIn = "https://www.linkedin.com/company/bosch-vietnam"
            },
            new
            {
                Name = "NAB Innovation Centre Vietnam",
                Description = "NAB technology hub building digital banking products, cloud platforms and data services.",
                Industry = Industry.Finance,
                Size = CompanySize.Enterprise,
                Website = "https://www.nab.com.au",
                LogoUrl = "https://logo.clearbit.com/nab.com.au",
                FoundedYear = 1982,
                Email = "vietnamcareers@nab.com.au",
                Phone = "+84 28 3822 8899",
                Address = "District 1, Ho Chi Minh City",
                LinkedIn = "https://www.linkedin.com/company/national-australia-bank"
            },
            new
            {
                Name = "SAP Labs Vietnam",
                Description = "SAP engineering teams in Vietnam delivering enterprise SaaS and AI-powered business applications.",
                Industry = Industry.Technology,
                Size = CompanySize.Enterprise,
                Website = "https://www.sap.com/vietnam",
                LogoUrl = "https://logo.clearbit.com/sap.com",
                FoundedYear = 1972,
                Email = "vietnam.recruiting@sap.com",
                Phone = "+84 28 3930 3333",
                Address = "Thu Duc, Ho Chi Minh City",
                LinkedIn = "https://www.linkedin.com/company/sap"
            },
        };

        foreach (var spec in companySpecs)
        {
            var existing = await context.Companies.FirstOrDefaultAsync(c => c.Name == spec.Name);
            if (existing is not null)
            {
                seeded.Add(existing);
                continue;
            }

            var contact = ContactInfo.Create(spec.Email, spec.Phone, spec.Address);
            if (contact.IsFailure)
            {
                continue;
            }

            var social = SocialLinks.Create(linkedIn: spec.LinkedIn);
            if (social.IsFailure)
            {
                continue;
            }

            var companyResult = Company.Register(
                spec.Name,
                spec.Description,
                spec.Industry,
                spec.Size,
                contact.Value,
                recruiterUserId,
                spec.Website,
                spec.LogoUrl,
                spec.FoundedYear,
                social.Value);

            if (companyResult.IsFailure)
            {
                continue;
            }

            var company = companyResult.Value;
            _ = company.Verify(SystemUserId);
            context.Companies.Add(company);
            seeded.Add(company);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Career seed: ensured {Count} companies.", seeded.Count);
        return seeded;
    }

    private static async Task EnsureRecruitersAsync(
        ApplicationDbContext context,
        ILogger logger,
        IReadOnlyDictionary<Company, Guid> companyRecruiterMap)
    {
        foreach (var pair in companyRecruiterMap)
        {
            var company = pair.Key;
            var recruiterUserId = pair.Value;
            var exists = await context.Recruiters.AnyAsync(r => r.CompanyId == company.Id && r.UserId == recruiterUserId);
            if (exists)
            {
                continue;
            }

            var recruiterResult = Recruiter.Add(
                recruiterUserId,
                company.Id,
                RecruiterPermissions.Admin(),
                SystemUserId);

            if (recruiterResult.IsFailure)
            {
                continue;
            }

            context.Recruiters.Add(recruiterResult.Value);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Career seed: ensured recruiters for seeded companies.");
    }

    private static async Task EnsureJobPostingsAsync(
        ApplicationDbContext context,
        ILogger logger,
        IReadOnlyList<Company> companies,
        IReadOnlyDictionary<Company, Guid> companyRecruiterMap)
    {
        var companyByName = companies.ToDictionary(c => c.Name, c => c);
        var specs = new[]
        {
            new
            {
                Title = "Backend .NET Engineer",
                CompanyName = "Bosch Global Software Technologies Vietnam",
                Description = """
## Mô tả công việc
Build scalable backend services with .NET, SQL and cloud-native architecture for global automotive platforms.
Develop APIs and integration components following secure coding standards.
Collaborate with product and QA teams to deliver features end-to-end.
Monitor, debug and optimize service performance in production-like environments.

## Hồ sơ của bạn
Final-year student or graduate in Computer Science / Software Engineering / IT.
Solid foundation in C#, ASP.NET Core, SQL and REST API design.
Understanding of clean architecture, testing and version control workflows.
Able to read technical documentation in English and communicate clearly in team settings.

## Liên hệ & thông tin bổ sung
Monthly allowance and structured mentorship from senior engineers.
Opportunity to work on real projects in an international engineering environment.
Only shortlisted candidates will be contacted.
""",
                JobType = JobType.FullTime,
                Exp = ExperienceLevel.Junior,
                City = "Ho Chi Minh City",
                District = "Tan Binh",
                IsRemote = false,
                Min = 25000000m,
                Max = 40000000m,
                Tags = new[] { "dotnet", "microservices", "sql" },
                Skills = new[] { "C#", "ASP.NET Core", "PostgreSQL" },
            },
            new
            {
                Title = "Cloud Platform Engineer",
                CompanyName = "NAB Innovation Centre Vietnam",
                Description = """
## Mô tả công việc
Design and operate cloud platform tooling, CI/CD and observability for digital banking products.
Implement infrastructure-as-code modules and improve deployment reliability.
Build internal platform capabilities for developer productivity and secure delivery.
Participate in incident response, root cause analysis and preventive improvements.

## Hồ sơ của bạn
Good hands-on knowledge of cloud platforms, containers and DevOps practices.
Experience with Kubernetes, Terraform or similar IaC tooling is preferred.
Strong problem-solving mindset and ability to work across engineering teams.
Comfortable reading English technical docs and writing clear operational notes.

## Liên hệ & thông tin bổ sung
Hybrid/remote-friendly setup depending on project needs.
Access to learning budget, internal guilds and mentorship.
Only shortlisted candidates will be contacted.
""",
                JobType = JobType.FullTime,
                Exp = ExperienceLevel.Mid,
                City = "Ho Chi Minh City",
                District = "District 1",
                IsRemote = true,
                Min = 35000000m,
                Max = 55000000m,
                Tags = new[] { "cloud", "devops", "platform" },
                Skills = new[] { "Kubernetes", "Terraform", "Azure" },
            },
            new
            {
                Title = "SAP Fullstack Developer Intern",
                CompanyName = "SAP Labs Vietnam",
                Description = """
## Mô tả công việc
Internship Duration: 02 Jul - 25 Dec 2026
Internship Model: Full-time or at least 4 days/week
Application Period: 4 May – 8 Jun 2026
Develop software features and components under supervision, adhering to coding standards.
Assist in database design, implementation, and query optimization.
Conduct testing, debugging, and troubleshooting to ensure code functionality and performance.
Create and maintain technical documentation for developed features.
Learn and apply new technologies, frameworks, and methodologies.
Support front-end and back-end integration, including basic security implementation.

## Hồ sơ của bạn
3rd or final-year student in Computer Science, Software Engineering, or related major.
Official university recommendation letter with stamp (mandatory).
Commitment: full-time or at least 4 days/week.
Good knowledge in Java/.NET/NodeJS/Python/Django.
Good knowledge in JavaScript/HTML/CSS; Angular/ReactJS is an advantage.
Fair English communication skills.

## Liên hệ & thông tin bổ sung
Graduates or applicants without a university recommendation letter are not eligible.
Monthly internship allowance + meal & parking allowance.
1 day paid leave per month and team-building activities.
Due to the high number of applications, only shortlisted candidates will be contacted.
""",
                JobType = JobType.Internship,
                Exp = ExperienceLevel.Entry,
                City = "Ho Chi Minh City",
                District = "Thu Duc",
                IsRemote = true,
                Min = 8000000m,
                Max = 15000000m,
                Tags = new[] { "internship", "java", "frontend" },
                Skills = new[] { "Java", "TypeScript", "React" },
            },
        };

        foreach (var spec in specs)
        {
            if (!companyByName.TryGetValue(spec.CompanyName, out var company))
            {
                continue;
            }
            var recruiterUserId = companyRecruiterMap.TryGetValue(company, out var mappedUserId)
                ? mappedUserId
                : SystemUserId;

            var exists = await context.JobPostings.AnyAsync(j => j.Title == spec.Title && j.CompanyId == company.Id.Value);
            if (exists)
            {
                continue;
            }

            var location = WorkLocation.Create(spec.City, spec.District, null, spec.IsRemote);
            if (location.IsFailure)
            {
                continue;
            }

            var salary = SalaryRange.Create(spec.Min, spec.Max, "VND", "month");
            if (salary.IsFailure)
            {
                continue;
            }

            var jobResult = JobPosting.Create(
                spec.Title,
                spec.Description,
                company.Id.Value,
                recruiterUserId,
                spec.JobType,
                spec.Exp,
                location.Value,
                salary.Value,
                DateTime.UtcNow.AddMonths(2));

            if (jobResult.IsFailure)
            {
                continue;
            }

            var job = jobResult.Value;
            foreach (var tag in spec.Tags)
            {
                _ = job.AddTag(tag);
            }

            foreach (var skill in spec.Skills)
            {
                var req = JobRequirement.Create(skill, true);
                if (req.IsSuccess)
                {
                    _ = job.AddRequirement(req.Value);
                }
            }

            _ = job.Publish();
            context.JobPostings.Add(job);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Career seed: ensured baseline job postings.");
    }

    private static async Task<IReadOnlyDictionary<Company, Guid>> BuildCompanyRecruiterMapAsync(
        ApplicationDbContext context,
        IReadOnlyList<Company> companies)
    {
        var preferredEmailsByCompanyName = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Bosch Global Software Technologies Vietnam"] = "bosch@unihub.edu.vn",
            ["NAB Innovation Centre Vietnam"] = "nab@unihub.edu.vn",
            ["SAP Labs Vietnam"] = "sap@unihub.edu.vn"
        };

        var map = new Dictionary<Company, Guid>();
        foreach (var company in companies)
        {
            Guid recruiterUserId = SystemUserId;
            if (preferredEmailsByCompanyName.TryGetValue(company.Name, out var emailRaw))
            {
                var email = Email.Create(emailRaw).Value;
                var recruiterUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
                if (recruiterUser is not null)
                {
                    recruiterUserId = recruiterUser.Id.Value;
                }
            }

            map[company] = recruiterUserId;
        }

        return map;
    }
}
