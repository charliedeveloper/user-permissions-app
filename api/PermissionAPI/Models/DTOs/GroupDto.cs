namespace PermissionAPI.Models.DTOs;

/// <summary>
/// Data transfer object for group
/// </summary>
public class GroupDto
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
}
