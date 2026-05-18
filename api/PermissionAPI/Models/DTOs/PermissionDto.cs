namespace PermissionAPI.Models.DTOs;

/// <summary>
/// Data transfer object for user permission
/// </summary>
public class PermissionDto
{
    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Unique permission key
    /// </summary>
    public string PermissionKey { get; set; } = string.Empty;

    /// <summary>
    /// Permission display name
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// Group name (if permission is from a group, null if direct permission)
    /// </summary>
    public string? GroupName { get; set; }
}
