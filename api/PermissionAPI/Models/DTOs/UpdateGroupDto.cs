using System.ComponentModel.DataAnnotations;

namespace PermissionAPI.Models.DTOs;

/// <summary>
/// DTO for updating an existing group
/// </summary>
public class UpdateGroupDto
{
    /// <summary>
    /// Group name (required, max 100 characters)
    /// </summary>
    [Required(ErrorMessage = "Group name is required")]
    [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters")]
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the group is active
    /// </summary>
    public bool IsActive { get; set; }
}
