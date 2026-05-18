using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermissionAPI.Models.DTOs;
using PermissionAPI.Services.Interfaces;

namespace PermissionAPI.Controllers;

/// <summary>
/// Permission management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(
        IPermissionService permissionService,
        ILogger<PermissionsController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all permissions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all permissions</returns>
    /// <response code="200">Returns the list of permissions</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PermissionDetailDto>>> GetAllPermissions(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionService.GetAllPermissionsAsync(cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Gets a specific permission by ID
    /// </summary>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Permission details</returns>
    /// <response code="200">Returns the permission</response>
    /// <response code="404">Permission not found</response>
    /// <response code="400">Invalid permission ID</response>
    [HttpGet("{permissionId:int}")]
    [ProducesResponseType(typeof(PermissionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PermissionDetailDto>> GetPermissionById(
        int permissionId,
        CancellationToken cancellationToken = default)
    {
        if (permissionId <= 0)
        {
            _logger.LogWarning("Invalid permissionId {PermissionId} provided", permissionId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid permission ID",
                Detail = "Permission ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var permission = await _permissionService.GetPermissionByIdAsync(permissionId, cancellationToken);

        if (permission == null)
        {
            _logger.LogWarning("Permission {PermissionId} not found", permissionId);
            return NotFound(new ProblemDetails
            {
                Title = "Permission not found",
                Detail = $"Permission with ID {permissionId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(permission);
    }

    /// <summary>
    /// Creates a new permission
    /// </summary>
    /// <param name="createPermissionDto">Permission creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created permission</returns>
    /// <response code="201">Permission created successfully</response>
    /// <response code="400">Invalid input data</response>
    [HttpPost]
    [ProducesResponseType(typeof(PermissionDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PermissionDetailDto>> CreatePermission(
        [FromBody] CreatePermissionDto createPermissionDto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var permission = await _permissionService.CreatePermissionAsync(createPermissionDto, cancellationToken);
            return CreatedAtAction(nameof(GetPermissionById), new { permissionId = permission.PermissionId }, permission);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error creating permission: {PermissionKey}", createPermissionDto.PermissionKey);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Permission creation failed",
                Detail = "A permission with this key may already exist",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Updates an existing permission
    /// </summary>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="updatePermissionDto">Permission update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Permission updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="404">Permission not found</response>
    [HttpPut("{permissionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePermission(
        int permissionId,
        [FromBody] UpdatePermissionDto updatePermissionDto,
        CancellationToken cancellationToken = default)
    {
        if (permissionId <= 0)
        {
            _logger.LogWarning("Invalid permissionId {PermissionId} provided", permissionId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid permission ID",
                Detail = "Permission ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _permissionService.UpdatePermissionAsync(permissionId, updatePermissionDto, cancellationToken);

            if (!updated)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Permission not found",
                    Detail = $"Permission with ID {permissionId} does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating permission {PermissionId}", permissionId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Permission update failed",
                Detail = "A permission with this key may already exist",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Deletes a permission
    /// </summary>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Permission deleted successfully</response>
    /// <response code="400">Invalid permission ID</response>
    /// <response code="404">Permission not found</response>
    [HttpDelete("{permissionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePermission(int permissionId, CancellationToken cancellationToken = default)
    {
        if (permissionId <= 0)
        {
            _logger.LogWarning("Invalid permissionId {PermissionId} provided", permissionId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid permission ID",
                Detail = "Permission ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var deleted = await _permissionService.DeletePermissionAsync(permissionId, cancellationToken);

        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Permission not found",
                Detail = $"Permission with ID {permissionId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }
}
