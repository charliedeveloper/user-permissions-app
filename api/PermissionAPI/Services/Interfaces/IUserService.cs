using PermissionAPI.Models.DTOs;

namespace PermissionAPI.Services.Interfaces;

/// <summary>
/// Service for user operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of users</returns>
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User DTO or null if not found</returns>
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);

    // User-Permission management (UserDirectPermissions)
    /// <summary>
    /// Assigns a direct permission to a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if assigned successfully, false if already assigned</returns>
    Task<bool> AssignDirectPermissionToUserAsync(int userId, int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a direct permission from a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if removed successfully, false if not found</returns>
    Task<bool> RemoveDirectPermissionFromUserAsync(int userId, int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets only direct permissions for a user (not group-based)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of direct permissions</returns>
    Task<IReadOnlyList<PermissionDetailDto>> GetUserDirectPermissionsAsync(int userId, CancellationToken cancellationToken = default);

    // User-Group management (UserGroups)
    /// <summary>
    /// Assigns a user to a group
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if assigned successfully, false if already assigned</returns>
    Task<bool> AssignUserToGroupAsync(int userId, int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user from a group
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="groupId">Group identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if removed successfully, false if not found</returns>
    Task<bool> RemoveUserFromGroupAsync(int userId, int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all groups a user belongs to
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of groups</returns>
    Task<IReadOnlyList<GroupDto>> GetUserGroupsAsync(int userId, CancellationToken cancellationToken = default);

    // Bulk assignment
    /// <summary>
    /// Assigns multiple groups and direct permissions to a user in a single operation
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="assignUserRolesDto">Bulk assignment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result with details</returns>
    Task<AssignmentResult> AssignUserRolesAsync(int userId, AssignUserRolesDto assignUserRolesDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs user's groups and direct permissions (sync/replace operation)
    /// Replaces the entire list of groups and permissions with the provided lists
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="syncUserRolesDto">Sync data containing desired groups and permissions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed sync result</returns>
    Task<UserRolesSyncResult> SyncUserRolesAsync(int userId, SyncUserRolesDto syncUserRolesDto, CancellationToken cancellationToken = default);
}
