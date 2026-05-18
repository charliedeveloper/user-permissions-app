namespace PermissionAPI.Models.DTOs;

/// <summary>
/// Result of bulk assignment operation
/// </summary>
public class AssignmentResult
{
    /// <summary>
    /// Number of groups successfully assigned
    /// </summary>
    public int GroupsAssigned { get; set; }

    /// <summary>
    /// Number of direct permissions successfully assigned
    /// </summary>
    public int PermissionsAssigned { get; set; }

    /// <summary>
    /// Groups that were already assigned (skipped)
    /// </summary>
    public List<int> SkippedGroupIds { get; set; } = new();

    /// <summary>
    /// Permissions that were already assigned (skipped)
    /// </summary>
    public List<int> SkippedPermissionIds { get; set; } = new();

    /// <summary>
    /// Groups that were not found
    /// </summary>
    public List<int> InvalidGroupIds { get; set; } = new();

    /// <summary>
    /// Permissions that were not found
    /// </summary>
    public List<int> InvalidPermissionIds { get; set; } = new();

    /// <summary>
    /// Indicates if the operation was completely successful
    /// </summary>
    public bool Success => InvalidGroupIds.Count == 0 && InvalidPermissionIds.Count == 0;
}
