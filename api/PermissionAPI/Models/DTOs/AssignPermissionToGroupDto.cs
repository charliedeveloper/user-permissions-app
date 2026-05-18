using System.ComponentModel.DataAnnotations;

namespace PermissionAPI.Models.DTOs;

/// <summary>
/// DTO for assigning a permission to a group
/// </summary>
public class AssignPermissionToGroupDto
{
    /// <summary>
    /// Permission identifier
    /// </summary>
    [Required(ErrorMessage = "Permission ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Permission ID must be a positive integer")]
    public int PermissionId { get; set; }
}
