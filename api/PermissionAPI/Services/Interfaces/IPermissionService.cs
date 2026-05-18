using PermissionAPI.Models.DTOs;

namespace PermissionAPI.Services.Interfaces;

/// <summary>
/// Service for permission operations
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets all permissions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permissions</returns>
    Task<IReadOnlyList<PermissionDetailDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a permission by ID
    /// </summary>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Permission DTO or null if not found</returns>
    Task<PermissionDetailDto?> GetPermissionByIdAsync(int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new permission
    /// </summary>
    /// <param name="createPermissionDto">Permission creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created permission DTO</returns>
    Task<PermissionDetailDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing permission
    /// </summary>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="updatePermissionDto">Permission update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully, false if not found</returns>
    Task<bool> UpdatePermissionAsync(int permissionId, UpdatePermissionDto updatePermissionDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a permission
    /// </summary>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    Task<bool> DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a permission exists
    /// </summary>
    /// <param name="permissionId">Permission identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if permission exists, false otherwise</returns>
    Task<bool> PermissionExistsAsync(int permissionId, CancellationToken cancellationToken = default);
}
