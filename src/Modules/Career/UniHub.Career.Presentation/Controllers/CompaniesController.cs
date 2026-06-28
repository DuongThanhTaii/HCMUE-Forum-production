using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Career.Application.Commands.Companies.RegisterCompany;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Application.Queries.Companies.GetCompanyById;
using UniHub.Career.Application.Queries.Companies.GetCompanyStatistics;
using UniHub.Career.Application.Queries.Companies.GetRecentApplications;
using UniHub.Career.Application.Queries.JobPostings.GetJobPostings;
using UniHub.Career.Presentation.Services;
using UniHub.Contracts;

namespace UniHub.Career.Presentation.Controllers;

[ApiController]
[Route("api/v1/companies")]
[Produces("application/json")]
[Authorize]
public class CompaniesController : BaseApiController
{
    private readonly ISender _sender;
    private readonly ICareerLogoStorageService _careerLogoStorageService;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRecruiterRepository _recruiterRepository;

    public CompaniesController(
        ISender sender,
        ICareerLogoStorageService careerLogoStorageService,
        ICompanyRepository companyRepository,
        IRecruiterRepository recruiterRepository)
    {
        _sender = sender;
        _careerLogoStorageService = careerLogoStorageService;
        _companyRepository = companyRepository;
        _recruiterRepository = recruiterRepository;
    }

    /// <summary>
    /// Register a new company
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CompanyResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command with { RegisteredBy = GetCurrentUserId() },
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.CompanyId },
            ApiResponses.Success(result.Value, "Company registered successfully"));
    }

    /// <summary>
    /// Upload company logo image and return hosted URL.
    /// </summary>
    [HttpPost("logo/upload")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadLogo([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponses.Failure("Logo file is required."));
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        var isImage = extension is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp";
        if (!isImage)
        {
            return BadRequest(ApiResponses.Failure("Only image files are supported for company logos."));
        }

        var url = await _careerLogoStorageService.UploadLogoAsync(file, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponses.Success(new { url }, "Company logo uploaded successfully"));
    }

    /// <summary>
    /// Get companies registered by current user.
    /// </summary>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var companies = await _companyRepository.GetByRegisteredByAsync(userId, cancellationToken);

        var recruiterLinks = await _recruiterRepository.GetByUserAsync(userId, cancellationToken);
        var recruiterCompanyIds = recruiterLinks.Select(r => r.CompanyId).Distinct().ToList();
        foreach (var companyId in recruiterCompanyIds)
        {
            var company = await _companyRepository.GetByIdAsync(CompanyId.Create(companyId.Value), cancellationToken);
            if (company is not null && companies.All(c => c.Id != company.Id))
            {
                companies.Add(company);
            }
        }

        var data = companies.Select(c => new
        {
            id = c.Id.Value,
            name = c.Name,
            status = c.Status.ToString(),
            logoUrl = c.LogoUrl
        }).ToList();
        return Ok(ApiResponses.Success((object)data));
    }

    /// <summary>
    /// Approve a pending company registration (Admin only)
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [RequirePermission("admin.system.manage")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var command = new Application.Commands.Companies.ApproveCompany.ApproveCompanyCommand(
            id,
            GetCurrentUserId());

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Company.NotFound"
                ? NotFound(ApiResponses.Failure(result.Error.Message))
                : BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Company approved successfully"));
    }

    /// <summary>
    /// Get company by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CompanyDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCompanyByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Get company statistics for dashboard
    /// </summary>
    [HttpGet("{id:guid}/statistics")]
    [ProducesResponseType(typeof(ApiResponse<CompanyStatisticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatistics(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCompanyStatisticsQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Get company's job postings
    /// </summary>
    [HttpGet("{id:guid}/jobs")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<JobPostingListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobs(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetJobPostingsQuery(
            Page: page,
            PageSize: pageSize,
            CompanyId: id,
            JobType: null,
            ExperienceLevel: null,
            Status: null,
            City: null,
            IsRemote: null,
            SearchTerm: null);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Get recent applications for company
    /// </summary>
    [HttpGet("{id:guid}/applications")]
    [ProducesResponseType(typeof(ApiResponse<RecentApplicationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecentApplications(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRecentApplicationsQuery(id, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }
}
