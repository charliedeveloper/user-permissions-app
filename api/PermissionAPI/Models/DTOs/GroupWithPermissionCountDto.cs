namespace PermissionAPI.Models.DTOs;

/// <summary>
/// Data transfer object for group with permission count
/// </summary>
public class GroupWithPermissionCountDto
{
    /// <summary>
    /// Group identifier
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Group name
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the group is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of permissions assigned to this group
    /// </summary>
    public int PermissionCount { get; set; }
}
