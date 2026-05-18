using PermissionAPI.Models.DTOs;

namespace PermissionAPI.Services.Interfaces;

/// <summary>
/// Service for retrieving user permissions
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// Gets all permissions for a user using stored procedure
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permission DTOs</returns>
    Task<IReadOnlyList<PermissionDto>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a user using LINQ (alternative implementation)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permission DTOs</returns>
    Task<IReadOnlyList<PermissionDto>> GetUserPermissionsLinqAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);
}
