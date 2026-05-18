using Microsoft.AspNetCore.Mvc;
using PermissionAPI.Models.DTOs;
using PermissionAPI.Services.Interfaces;

namespace PermissionAPI.Controllers;

/// <summary>
/// User management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserPermissionService _userPermissionService;
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserPermissionService userPermissionService,
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userPermissionService = userPermissionService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all users</returns>
    /// <response code="200">Returns the list of users</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAllUsers(CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Gets a specific user by ID
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid user ID</response>
    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> GetUserById(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} provided", userId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid user ID",
                Detail = "User ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var user = await _userService.GetUserByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {userId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(user);
    }

    /// <summary>
    /// Gets all permissions for a specific user
    /// </summary>
    /// <param name="userId">The unique user identifier</param>
    /// <param name="useLinq">Optional: use LINQ implementation instead of stored procedure (default: false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permissions assigned to the user</returns>
    /// <response code="200">Returns the list of user permissions</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid user ID</response>
    [HttpGet("{userId:int}/permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PermissionDto>>> GetUserPermissions(
        int userId,
        [FromQuery] bool useLinq = false,
        CancellationToken cancellationToken = default)
    {
        // Validation: userId must be positive
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} provided", userId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid user ID",
                Detail = "User ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Check if user exists
        var userExists = await _userPermissionService.UserExistsAsync(userId, cancellationToken);
        if (!userExists)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {userId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Call appropriate service method
        var permissions = useLinq
            ? await _userPermissionService.GetUserPermissionsLinqAsync(userId, cancellationToken)
            : await _userPermissionService.GetUserPermissionsAsync(userId, cancellationToken);

        return Ok(permissions);
    }

    /// <summary>
    /// Gets all groups a user belongs to
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of groups</returns>
    /// <response code="200">Returns the list of groups</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid user ID</response>
    [HttpGet("{userId:int}/groups")]
    [ProducesResponseType(typeof(IReadOnlyList<GroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<GroupDto>>> GetUserGroups(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} provided", userId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid user ID",
                Detail = "User ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var userExists = await _userService.UserExistsAsync(userId, cancellationToken);
        if (!userExists)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {userId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        var groups = await _userService.GetUserGroupsAsync(userId, cancellationToken);
        return Ok(groups);
    }

    /// <summary>
    /// Assigns a user to a group
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="assignUserToGroupDto">Group assignment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">User assigned to group successfully</response>
    /// <response code="400">Invalid input or user already in group</response>
    /// <response code="404">User or group not found</response>
    [HttpPost("{userId:int}/groups")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignUserToGroup(
        int userId,
        [FromBody] AssignUserToGroupDto assignUserToGroupDto,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} provided", userId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid user ID",
                Detail = "User ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var assigned = await _userService.AssignUserToGroupAsync(userId, assignUserToGroupDto.GroupId, cancellationToken);

        if (!assigned)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Assignment failed",
                Detail = $"User {userId} may already be in group {assignUserToGroupDto.GroupId}, or user/group doesn't exist",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Removes a user from a group
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">User removed from group successfully</response>
    /// <response code="400">Invalid IDs</response>
    /// <response code="404">User, group, or association not found</response>
    [HttpDelete("{userId:int}/groups/{groupId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUserFromGroup(
        int userId,
        int groupId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || groupId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} or groupId {GroupId} provided", userId, groupId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid IDs",
                Detail = "User ID and Group ID must be positive integers",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var removed = await _userService.RemoveUserFromGroupAsync(userId, groupId, cancellationToken);

        if (!removed)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Removal failed",
                Detail = $"User {userId} is not in group {groupId}, or user doesn't exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Gets only direct permissions for a user (not group-based)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of direct permissions</returns>
    /// <response code="200">Returns the list of direct permissions</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid user ID</response>
    [HttpGet("{userId:int}/permissions/direct")]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PermissionDetailDto>>> GetUserDirectPermissions(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} provided", userId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid user ID",
                Detail = "User ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var userExists = await _userService.UserExistsAsync(userId, cancellationToken);
        if (!userExists)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {userId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        var permissions = await _userService.GetUserDirectPermissionsAsync(userId, cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Assigns a direct permission to a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="assignPermissionDto">Permission assignment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Permission assigned successfully</response>
    /// <response code="400">Invalid input or permission already assigned</response>
    /// <response code="404">User or permission not found</response>
    [HttpPost("{userId:int}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDirectPermissionToUser(
        int userId,
        [FromBody] AssignPermissionToUserDto assignPermissionDto,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} provided", userId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid user ID",
                Detail = "User ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var assigned = await _userService.AssignDirectPermissionToUserAsync(userId, assignPermissionDto.PermissionId, cancellationToken);

        if (!assigned)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Assignment failed",
                Detail = $"Permission {assignPermissionDto.PermissionId} may already be assigned to user {userId}, or user/permission doesn't exist",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Removes a direct permission from a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Permission removed successfully</response>
    /// <response code="400">Invalid IDs</response>
    /// <response code="404">User, permission, or association not found</response>
    [HttpDelete("{userId:int}/permissions/{permissionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDirectPermissionFromUser(
        int userId,
        int permissionId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || permissionId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} or permissionId {PermissionId} provided", userId, permissionId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid IDs",
                Detail = "User ID and Permission ID must be positive integers",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var removed = await _userService.RemoveDirectPermissionFromUserAsync(userId, permissionId, cancellationToken);

        if (!removed)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Removal failed",
                Detail = $"Permission {permissionId} is not assigned to user {userId}, or user doesn't exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Assigns multiple groups and direct permissions to a user in a single operation
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="assignUserRolesDto">Bulk assignment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result with details</returns>
    /// <response code="200">Returns assignment result</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="404">User not found</response>
    [HttpPost("{userId:int}/roles")]
    [ProducesResponseType(typeof(AssignmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentResult>> AssignUserRoles(
        int userId,
        [FromBody] AssignUserRolesDto assignUserRolesDto,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} provided", userId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid user ID",
                Detail = "User ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _userService.AssignUserRolesAsync(userId, assignUserRolesDto, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Bulk assignment partially failed for user {UserId}: {InvalidGroups} invalid groups, {InvalidPermissions} invalid permissions",
                    userId,
                    result.InvalidGroupIds.Count,
                    result.InvalidPermissionIds.Count);

                return Ok(result); // Return 200 with details about what failed
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Syncs user's groups and direct permissions (replaces entire lists)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="syncUserRolesDto">Sync data containing desired groups and permissions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed sync result</returns>
    /// <response code="200">Returns sync result with details</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="404">User not found</response>
    [HttpPut("{userId:int}/roles/sync")]
    [ProducesResponseType(typeof(UserRolesSyncResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserRolesSyncResult>> SyncUserRoles(
        int userId,
        [FromBody] SyncUserRolesDto syncUserRolesDto,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId {UserId} provided", userId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid user ID",
                Detail = "User ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _userService.SyncUserRolesAsync(userId, syncUserRolesDto, cancellationToken);

            _logger.LogInformation(
                "User {UserId} roles synced: {TotalChanges} total changes, Success: {Success}",
                userId,
                result.TotalChanges,
                result.Success);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}
