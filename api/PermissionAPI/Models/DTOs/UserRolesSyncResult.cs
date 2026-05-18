namespace PermissionAPI.Models.DTOs;

/// <summary>
/// Result of user roles sync operation (groups and direct permissions)
/// </summary>
public class UserRolesSyncResult
{
    // Group sync results
    /// <summary>
    /// Number of groups added
    /// </summary>
    public int GroupsAdded { get; set; }

    /// <summary>
    /// Number of groups removed
    /// </summary>
    public int GroupsRemoved { get; set; }

    /// <summary>
    /// Number of groups unchanged
    /// </summary>
    public int GroupsUnchanged { get; set; }

    /// <summary>
    /// Invalid group IDs that don't exist
    /// </summary>
    public List<int> InvalidGroupIds { get; set; } = new();

    /// <summary>
    /// Final list of valid group IDs after sync
    /// </summary>
    public List<int> FinalGroupIds { get; set; } = new();

    // Permission sync results
    /// <summary>
    /// Number of direct permissions added
    /// </summary>
    public int PermissionsAdded { get; set; }

    /// <summary>
    /// Number of direct permissions removed
    /// </summary>
    public int PermissionsRemoved { get; set; }

    /// <summary>
    /// Number of direct permissions unchanged
    /// </summary>
    public int PermissionsUnchanged { get; set; }

    /// <summary>
    /// Invalid permission IDs that don't exist
    /// </summary>
    public List<int> InvalidPermissionIds { get; set; } = new();

    /// <summary>
    /// Final list of valid permission IDs after sync
    /// </summary>
    public List<int> FinalPermissionIds { get; set; } = new();

    /// <summary>
    /// Indicates if the operation was completely successful (no invalid IDs)
    /// </summary>
    public bool Success => InvalidGroupIds.Count == 0 && InvalidPermissionIds.Count == 0;

    /// <summary>
    /// Total number of changes made
    /// </summary>
    public int TotalChanges => GroupsAdded + GroupsRemoved + PermissionsAdded + PermissionsRemoved;
}
