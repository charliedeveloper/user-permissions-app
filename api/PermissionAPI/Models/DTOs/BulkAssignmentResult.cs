namespace PermissionAPI.Models.DTOs;

/// <summary>
/// Result of bulk permission assignment operation
/// </summary>
public class BulkAssignmentResult
{
    /// <summary>
    /// Number of permissions added
    /// </summary>
    public int PermissionsAdded { get; set; }

    /// <summary>
    /// Number of permissions removed
    /// </summary>
    public int PermissionsRemoved { get; set; }

    /// <summary>
    /// Number of permissions that were already assigned (unchanged)
    /// </summary>
    public int PermissionsUnchanged { get; set; }

    /// <summary>
    /// Permission IDs that were not found
    /// </summary>
    public List<int> InvalidPermissionIds { get; set; } = new();

    /// <summary>
    /// Final list of permission IDs assigned to the group
    /// </summary>
    public List<int> FinalPermissionIds { get; set; } = new();

    /// <summary>
    /// Indicates if the operation was completely successful
    /// </summary>
    public bool Success => InvalidPermissionIds.Count == 0;
}