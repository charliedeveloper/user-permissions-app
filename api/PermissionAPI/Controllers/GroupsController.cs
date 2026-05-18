using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermissionAPI.Models.DTOs;
using PermissionAPI.Services.Interfaces;

namespace PermissionAPI.Controllers;

/// <summary>
/// Group management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
//[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(
        IGroupService groupService,
        IPermissionService permissionService,
        ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all groups
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all groups</returns>
    /// <response code="200">Returns the list of groups</response>
    [HttpGet]

    [ProducesResponseType(typeof(IReadOnlyList<GroupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GroupDto>>> GetAllGroups(CancellationToken cancellationToken = default)
    {
        var groups = await _groupService.GetAllGroupsAsync(cancellationToken);
        return Ok(groups);
    }

    /// <summary>
    /// Gets all groups with their permission counts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all groups with permission counts</returns>
    /// <response code="200">Returns the list of groups with permission counts</response>
    [HttpGet("with-permission-counts")]
    [ProducesResponseType(typeof(IReadOnlyList<GroupWithPermissionCountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GroupWithPermissionCountDto>>> GetGroupsWithPermissionCounts(CancellationToken cancellationToken = default)
    {
        var groups = await _groupService.GetGroupsWithPermissionCountAsync(cancellationToken);
        return Ok(groups);
    }

    /// <summary>
    /// Gets a specific group by ID
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Group details</returns>
    /// <response code="200">Returns the group</response>
    /// <response code="404">Group not found</response>
    /// <response code="400">Invalid group ID</response>
    [HttpGet("{groupId:int}")]
    [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    
    public async Task<ActionResult<GroupDto>> GetGroupById(int groupId, CancellationToken cancellationToken = default)
    {
        if (groupId <= 0)
        {
            _logger.LogWarning("Invalid groupId {GroupId} provided", groupId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid group ID",
                Detail = "Group ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var group = await _groupService.GetGroupByIdAsync(groupId, cancellationToken);

        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return NotFound(new ProblemDetails
            {
                Title = "Group not found",
                Detail = $"Group with ID {groupId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(group);
    }

    /// <summary>
    /// Creates a new group
    /// </summary>
    /// <param name="createGroupDto">Group creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created group</returns>
    /// <response code="201">Group created successfully</response>
    /// <response code="400">Invalid input data</response>
    [HttpPost]
    [ProducesResponseType(typeof(GroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GroupDto>> CreateGroup(
        [FromBody] CreateGroupDto createGroupDto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var group = await _groupService.CreateGroupAsync(createGroupDto, cancellationToken);
            return CreatedAtAction(nameof(GetGroupById), new { groupId = group.GroupId }, group);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error creating group: {GroupName}", createGroupDto.GroupName);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Group creation failed",
                Detail = "A group with this name may already exist",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Updates an existing group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="updateGroupDto">Group update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Group updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="404">Group not found</response>
    [HttpPut("{groupId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGroup(
        int groupId,
        [FromBody] UpdateGroupDto updateGroupDto,
        CancellationToken cancellationToken = default)
    {
        if (groupId <= 0)
        {
            _logger.LogWarning("Invalid groupId {GroupId} provided", groupId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid group ID",
                Detail = "Group ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _groupService.UpdateGroupAsync(groupId, updateGroupDto, cancellationToken);

            if (!updated)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Group not found",
                    Detail = $"Group with ID {groupId} does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", groupId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Group update failed",
                Detail = "A group with this name may already exist",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Deletes a group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Group deleted successfully</response>
    /// <response code="400">Invalid group ID</response>
    /// <response code="404">Group not found</response>
    [HttpDelete("{groupId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGroup(int groupId, CancellationToken cancellationToken = default)
    {
        if (groupId <= 0)
        {
            _logger.LogWarning("Invalid groupId {GroupId} provided", groupId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid group ID",
                Detail = "Group ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var deleted = await _groupService.DeleteGroupAsync(groupId, cancellationToken);

        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Group not found",
                Detail = $"Group with ID {groupId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Gets all permissions assigned to a group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permissions</returns>
    /// <response code="200">Returns the list of permissions</response>
    /// <response code="404">Group not found</response>
    /// <response code="400">Invalid group ID</response>
    [HttpGet("{groupId:int}/permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PermissionDetailDto>>> GetGroupPermissions(
        int groupId,
        CancellationToken cancellationToken = default)
    {
        if (groupId <= 0)
        {
            _logger.LogWarning("Invalid groupId {GroupId} provided", groupId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid group ID",
                Detail = "Group ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var groupExists = await _groupService.GroupExistsAsync(groupId, cancellationToken);
        if (!groupExists)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return NotFound(new ProblemDetails
            {
                Title = "Group not found",
                Detail = $"Group with ID {groupId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        var permissions = await _groupService.GetGroupPermissionsAsync(groupId, cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Assigns a permission to a group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="assignPermissionDto">Permission assignment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Permission assigned successfully</response>
    /// <response code="400">Invalid input or permission already assigned</response>
    /// <response code="404">Group or permission not found</response>
    [HttpPost("{groupId:int}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignPermissionToGroup(
        int groupId,
        [FromBody] AssignPermissionToGroupDto assignPermissionDto,
        CancellationToken cancellationToken = default)
    {
        if (groupId <= 0)
        {
            _logger.LogWarning("Invalid groupId {GroupId} provided", groupId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid group ID",
                Detail = "Group ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if group exists
        var groupExists = await _groupService.GroupExistsAsync(groupId, cancellationToken);
        if (!groupExists)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return NotFound(new ProblemDetails
            {
                Title = "Group not found",
                Detail = $"Group with ID {groupId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Check if permission exists
        var permissionExists = await _permissionService.PermissionExistsAsync(assignPermissionDto.PermissionId, cancellationToken);
        if (!permissionExists)
        {
            _logger.LogWarning("Permission {PermissionId} not found", assignPermissionDto.PermissionId);
            return NotFound(new ProblemDetails
            {
                Title = "Permission not found",
                Detail = $"Permission with ID {assignPermissionDto.PermissionId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        var assigned = await _groupService.AssignPermissionToGroupAsync(groupId, assignPermissionDto.PermissionId, cancellationToken);

        if (!assigned)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Permission already assigned",
                Detail = $"Permission {assignPermissionDto.PermissionId} is already assigned to group {groupId}",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Removes a permission from a group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Permission removed successfully</response>
    /// <response code="400">Invalid IDs</response>
    /// <response code="404">Group, permission, or association not found</response>
    [HttpDelete("{groupId:int}/permissions/{permissionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePermissionFromGroup(
        int groupId,
        int permissionId,
        CancellationToken cancellationToken = default)
    {
        if (groupId <= 0 || permissionId <= 0)
        {
            _logger.LogWarning("Invalid groupId {GroupId} or permissionId {PermissionId} provided", groupId, permissionId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid IDs",
                Detail = "Group ID and Permission ID must be positive integers",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Check if group exists
        var groupExists = await _groupService.GroupExistsAsync(groupId, cancellationToken);
        if (!groupExists)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return NotFound(new ProblemDetails
            {
                Title = "Group not found",
                Detail = $"Group with ID {groupId} does not exist",
                Status = StatusCodes.Status404NotFound
            });
        }

        var removed = await _groupService.RemovePermissionFromGroupAsync(groupId, permissionId, cancellationToken);

        if (!removed)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Permission not found",
                Detail = $"Permission {permissionId} is not assigned to group {groupId}",
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Syncs permissions for a group (replaces all existing permissions)
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="bulkAssignDto">List of permission IDs to assign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of bulk assignment operation</returns>
    /// <response code="200">Permissions synced successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="404">Group not found</response>
    [HttpPut("{groupId:int}/permissions/sync")]
    [ProducesResponseType(typeof(BulkAssignmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BulkAssignmentResult>> SyncGroupPermissions(
        int groupId,
        [FromBody] BulkAssignPermissionsToGroupDto bulkAssignDto,
        CancellationToken cancellationToken = default)
    {
        if (groupId <= 0)
        {
            _logger.LogWarning("Invalid groupId {GroupId} provided", groupId);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid group ID",
                Detail = "Group ID must be a positive integer",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _groupService.SyncGroupPermissionsAsync(
                groupId, 
                bulkAssignDto.PermissionIds, 
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Group {GroupId} not found", groupId);
            return NotFound(new ProblemDetails
            {
                Title = "Group not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing permissions for group {GroupId}", groupId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal server error",
                Detail = "An error occurred while syncing permissions",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
