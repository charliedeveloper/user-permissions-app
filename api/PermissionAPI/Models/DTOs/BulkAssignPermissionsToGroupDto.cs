using System.ComponentModel.DataAnnotations;

namespace PermissionAPI.Models.DTOs;

/// <summary>
/// Bulk assignment of permissions to a group (replaces existing permissions)
/// </summary>
public class BulkAssignPermissionsToGroupDto
{
    /// <summary>
    /// List of permission IDs to assign to the group
    /// </summary>
    [Required]
    public List<int> PermissionIds { get; set; } = new();
}