using System.ComponentModel.DataAnnotations;

namespace PermissionAPI.Models.DTOs;

/// <summary>
/// DTO for syncing user's groups and direct permissions (sync/replace operation)
/// </summary>
public class SyncUserRolesDto
{
    /// <summary>
    /// List of group IDs the user should belong to (replaces existing)
    /// </summary>
    [Required]
    public List<int> GroupIds { get; set; } = new();

    /// <summary>
    /// List of permission IDs to assign directly to the user (replaces existing)
    /// </summary>
    [Required]
    public List<int> PermissionIds { get; set; } = new();
}
