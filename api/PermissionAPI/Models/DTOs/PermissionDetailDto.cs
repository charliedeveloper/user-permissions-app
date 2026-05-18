namespace PermissionAPI.Models.DTOs;

/// <summary>
/// Detailed data transfer object for permission
/// </summary>
public class PermissionDetailDto
{
    /// <summary>
    /// Permission identifier
    /// </summary>
    public int PermissionId { get; set; }

    /// <summary>
    /// Unique permission key
    /// </summary>
    public string PermissionKey { get; set; } = string.Empty;

    /// <summary>
    /// Permission name
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the permission is active
    /// </summary>
    public bool IsActive { get; set; }
}
