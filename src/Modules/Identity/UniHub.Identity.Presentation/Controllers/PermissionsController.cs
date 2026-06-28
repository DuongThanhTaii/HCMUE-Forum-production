using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Presentation.DTOs.Permissions;

namespace UniHub.Identity.Presentation.Controllers;

[ApiController]
[Route("api/v1/permissions")]
[Produces("application/json")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionsController(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permissions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        var response = permissions.Select(MapToPermissionResponse);
        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Get permission by ID
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Permission information</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByIdAsync(
            new PermissionId(id),
            cancellationToken);

        if (permission is null)
        {
            return NotFound(ApiResponses.Failure("Permission not found"));
        }

        return Ok(ApiResponses.Success(MapToPermissionResponse(permission)));
    }

    private static PermissionResponse MapToPermissionResponse(Permission permission)
    {
        return new PermissionResponse(
            permission.Id.Value,
            permission.Code,
            permission.Name,
            permission.Description,
            permission.Module,
            permission.Resource,
            permission.Action);
    }
}
