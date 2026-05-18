using PermissionAPI.Models.DTOs;

namespace PermissionAPI.Services.Interfaces;

/// <summary>
/// Service for group operations
/// </summary>
public interface IGroupService
{
    /// <summary>
    /// Gets all groups
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of groups</returns>
    Task<IReadOnlyList<GroupDto>> GetAllGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a group by ID
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Group DTO or null if not found</returns>
    Task<GroupDto?> GetGroupByIdAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new group
    /// </summary>
    /// <param name="createGroupDto">Group creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created group DTO</returns>
    Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="updateGroupDto">Group update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully, false if not found</returns>
    Task<bool> UpdateGroupAsync(int groupId, UpdateGroupDto updateGroupDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    Task<bool> DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a group exists
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if group exists, false otherwise</returns>
    Task<bool> GroupExistsAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a permission to a group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if assigned successfully, false if already assigned</returns>
    Task<bool> AssignPermissionToGroupAsync(int groupId, int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a permission from a group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if removed successfully, false if not found</returns>
    Task<bool> RemovePermissionFromGroupAsync(int groupId, int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a group
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permissions</returns>
    Task<IReadOnlyList<PermissionDetailDto>> GetGroupPermissionsAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all groups with their permission counts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of groups with permission counts</returns>
    Task<IReadOnlyList<GroupWithPermissionCountDto>> GetGroupsWithPermissionCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs permissions for a group (replaces all existing permissions)
    /// </summary>
    /// <param name="groupId">Group identifier</param>
    /// <param name="permissionIds">List of permission IDs to assign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of bulk assignment operation</returns>
    Task<BulkAssignmentResult> SyncGroupPermissionsAsync(int groupId, List<int> permissionIds, CancellationToken cancellationToken = default);
}
